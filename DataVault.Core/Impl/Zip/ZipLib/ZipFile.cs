using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Reflection;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Collections;
using DataVault.Core.Impl.Zip.ZipLib.Exceptions;
using DataVault.Core.Impl.Zip.ZipLib.Streams;
using System.Linq;

namespace DataVault.Core.Impl.Zip.ZipLib
{
    // todo. make this stuff fully thread-safe
    // this fix only guarantees multiple readers safety

    internal class ZipFile : IEnumerable<ZipEntry>, IDisposable
    {
        // Fields
        private bool _CaseSensitiveRetrieval;
        private String _Comment;
        private bool _contentsChanged;
        private List<ZipDirEntry> _direntries;
        private bool _disposed;
        private Encoding _encoding;
        private bool _fileAlreadyExists;
        private bool _ForceNoCompression;
        private bool _JustSaved;
        private String _name;
        private Stream _readstream;
        private bool _ReadStreamIsOurs;
        private bool _saveOperationCanceled;
        private TextWriter _StatusMessageTextWriter;
        private String _TempFileFolder;
        private String _temporaryFileName;
        private bool _TrimVolumeFromFullyQualifiedPaths;
        private Stream _writestream;
        public static readonly Encoding DefaultEncoding = Encoding.GetEncoding("IBM437");

        // Index for the _entries collection
        private TrackableList<ZipEntry> _entries;
        private Dictionary<String, ZipEntry> _entriesIndex = new Dictionary<String, ZipEntry>();

        private void InitializeEntries(bool startTrackingImmediately)
        {
            this._entries = new TrackableList<ZipEntry>();
            if (startTrackingImmediately) this._entries.ListChanged += EntriesChanged;
        }

        private void IndexEntriesAndTrackChangesSinceNow()
        {
            _entriesIndex = _entries.ToDictionary(ze => ze.FileName, ze => ze);
            this._entries.ListChanged += EntriesChanged;
        }

        private void EntriesChanged(Object sender, ItemListChangeEventArgs<ZipEntry> e)
        {
            e.RemovedItems.ForEach(ze => _entriesIndex.Remove(ze.FileName));
            e.AddedItems.ForEach(ze => _entriesIndex.Add(ze.FileName, ze));
        }

        // Methods
        public ZipFile()
        {
            this._TrimVolumeFromFullyQualifiedPaths = true;
            this._ReadStreamIsOurs = true;
            this._encoding = Encoding.GetEncoding("IBM437");
            this.InitFile(null, null);
        }

        public ZipFile(Stream outputStream)
        {
            this._TrimVolumeFromFullyQualifiedPaths = true;
            this._ReadStreamIsOurs = true;
            this._encoding = Encoding.GetEncoding("IBM437");
            if (!outputStream.CanWrite)
            {
                throw new ArgumentException("The outputStream must be a writable stream.");
            }
            this._writestream = new CountingStream(outputStream);
            InitializeEntries(true);
        }

        public ZipFile(String zipFileName, Encoding encoding)
        {
            this._TrimVolumeFromFullyQualifiedPaths = true;
            this._ReadStreamIsOurs = true;
            this._encoding = encoding;
            try
            {
                this.InitFile(zipFileName, null);
            }
            catch (Exception e1)
            {
                throw new ZipException(String.Format("{0} is not a valid zip file", zipFileName), e1);
            }
        }

        public ZipFile(String zipFileName)
            : this(zipFileName, Encoding.GetEncoding("IBM437"))
        {
        }

        public ZipFile(Stream outputStream, TextWriter statusMessageWriter)
        {
            this._TrimVolumeFromFullyQualifiedPaths = true;
            this._ReadStreamIsOurs = true;
            this._encoding = Encoding.GetEncoding("IBM437");
            if (!outputStream.CanWrite)
            {
                throw new ArgumentException("The outputStream must be a writable stream.");
            }
            this._writestream = new CountingStream(outputStream);
            InitializeEntries(true);
            this._StatusMessageTextWriter = statusMessageWriter;
        }

        public ZipFile(String zipFileName, Encoding encoding, TextWriter statusMessageWriter)
        {
            this._TrimVolumeFromFullyQualifiedPaths = true;
            this._ReadStreamIsOurs = true;
            this._encoding = encoding;
            try
            {
                this.InitFile(zipFileName, statusMessageWriter);
            }
            catch (Exception e1)
            {
                throw new ZipException(String.Format("{0} is not a valid zip file", zipFileName), e1);
            }
        }

        public ZipFile(String zipFileName, TextWriter statusMessageWriter)
            : this(zipFileName, Encoding.GetEncoding("IBM437"), statusMessageWriter)
        {
        }

        public void AddDirectory(String directoryName)
        {
            this.AddDirectory(directoryName, null);
        }

        public void AddDirectory(String directoryName, String directoryPathInArchive)
        {
            this.AddOrUpdateDirectoryImpl(directoryName, directoryPathInArchive, AddOrUpdateAction.AddOnly);
        }

        public ZipEntry AddDirectoryByName(String directoryNameInArchive)
        {
            ZipEntry baseDir = ZipEntry.Create(directoryNameInArchive, directoryNameInArchive);
            baseDir.TrimVolumeFromFullyQualifiedPaths = this.TrimVolumeFromFullyQualifiedPaths;
            baseDir._Source = EntrySource.Filesystem;
            baseDir.MarkAsDirectory();
            baseDir.Encoding = this.Encoding;
            this.InsureUniqueEntry(baseDir);
            this._entries.Add(baseDir);
            this._contentsChanged = true;
            return baseDir;
        }

        public ZipEntry AddFile(String fileName)
        {
            return this.AddFile(fileName, null);
        }

        public ZipEntry AddFile(String fileName, String directoryPathInArchive)
        {
            String nameInArchive = ZipEntry.NameInArchive(fileName, directoryPathInArchive);
            ZipEntry ze = ZipEntry.Create(fileName, nameInArchive);
            ze.TrimVolumeFromFullyQualifiedPaths = this.TrimVolumeFromFullyQualifiedPaths;
            ze.ForceNoCompression = this.ForceNoCompression;
            ze.Encoding = this.Encoding;
            ze._Source = EntrySource.Filesystem;
            if (this.Verbose)
            {
                this.StatusMessageTextWriter.WriteLine("adding {0}...", fileName);
            }
            this.InsureUniqueEntry(ze);
            this._entries.Add(ze);
            this._contentsChanged = true;
            return ze;
        }

        public ZipEntry AddFileFromString(String fileName, String directoryPathInArchive, String content)
        {
            return this.AddFileStream(fileName, directoryPathInArchive, content.AsStream());
        }

        public ZipEntry AddFileStream(String fileName, String directoryPathInArchive, Stream stream)
        {
            String n = ZipEntry.NameInArchive(fileName, directoryPathInArchive);
            // todo. this is so thread-unsafe that I'm cba to fix, so just caching it (potential memory hit!)
            ZipEntry ze = ZipEntry.Create(fileName, n, stream.CacheInMemory());
            ze.TrimVolumeFromFullyQualifiedPaths = this.TrimVolumeFromFullyQualifiedPaths;
            ze.ForceNoCompression = this.ForceNoCompression;
            ze.Encoding = this.Encoding;
            ze._Source = EntrySource.Stream;
            if (this.Verbose)
            {
                this.StatusMessageTextWriter.WriteLine("adding {0}...", fileName);
            }
            this.InsureUniqueEntry(ze);
            this._entries.Add(ze);
            this._contentsChanged = true;
            return ze;
        }

        public void AddItem(String fileOrDirectoryName)
        {
            this.AddItem(fileOrDirectoryName, null);
        }

        public void AddItem(String fileOrDirectoryName, String directoryPathInArchive)
        {
            if (File.Exists(fileOrDirectoryName))
            {
                this.AddFile(fileOrDirectoryName, directoryPathInArchive);
            }
            else
            {
                if (!Directory.Exists(fileOrDirectoryName))
                {
                    throw new FileNotFoundException(String.Format("That file or directory ({0}) does not exist!", fileOrDirectoryName));
                }
                this.AddDirectory(fileOrDirectoryName, directoryPathInArchive);
            }
        }

        private void AddOrUpdateDirectoryImpl(String directoryName, String rootDirectoryPathInArchive, AddOrUpdateAction action)
        {
            if (rootDirectoryPathInArchive == null)
            {
                rootDirectoryPathInArchive = "";
            }
            this.AddOrUpdateDirectoryImpl(directoryName, rootDirectoryPathInArchive, action, 0);
        }

        private void AddOrUpdateDirectoryImpl(String directoryName, String rootDirectoryPathInArchive, AddOrUpdateAction action, int level)
        {
            if (this.Verbose)
            {
                this.StatusMessageTextWriter.WriteLine("{0} {1}...", (action == AddOrUpdateAction.AddOnly) ? "adding" : "Adding or updating", directoryName);
            }
            String dirForEntries = rootDirectoryPathInArchive;
            if (level > 0)
            {
                int f = directoryName.Length;
                for (int i = level; i > 0; i--)
                {
                    f = directoryName.LastIndexOfAny(@"/\".ToCharArray(), f - 1, f - 1);
                }
                dirForEntries = directoryName.Substring(f + 1);
                dirForEntries = Path.Combine(rootDirectoryPathInArchive, dirForEntries);
            }
            if ((level > 0) || (rootDirectoryPathInArchive != ""))
            {
                ZipEntry baseDir = ZipEntry.Create(directoryName, dirForEntries);
                baseDir.Encoding = this.Encoding;
                baseDir.TrimVolumeFromFullyQualifiedPaths = this.TrimVolumeFromFullyQualifiedPaths;
                baseDir._Source = EntrySource.Filesystem;
                baseDir.MarkAsDirectory();
                ZipEntry e = this[baseDir.FileName];
                if (e == null)
                {
                    this._entries.Add(baseDir);
                    this._contentsChanged = true;
                }
                dirForEntries = baseDir.FileName;
            }
            String[] filenames = Directory.GetFiles(directoryName);
            foreach (String filename in filenames)
            {
                if (action == AddOrUpdateAction.AddOnly)
                {
                    this.AddFile(filename, dirForEntries);
                }
                else
                {
                    this.UpdateFile(filename, dirForEntries);
                }
            }
            String[] dirnames = Directory.GetDirectories(directoryName);
            foreach (String dir in dirnames)
            {
                this.AddOrUpdateDirectoryImpl(dir, rootDirectoryPathInArchive, action, level + 1);
            }
            this._contentsChanged = true;
        }

        private void CleanupAfterSaveOperation()
        {
            if ((this._temporaryFileName != null) && (this._name != null))
            {
                if (this._writestream != null)
                {
                    try
                    {
                        this._writestream.Close();
                    }
                    catch
                    {
                    }
                    try
                    {
                        this._writestream.Dispose();
                    }
                    catch
                    {
                    }
                }
                this._writestream = null;
                this.RemoveTempFile();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposeManagedResources)
        {
            if (!this._disposed)
            {
                if (disposeManagedResources)
                {
                    if (this._ReadStreamIsOurs && (this._readstream != null))
                    {
                        this._readstream.Dispose();
                        this._readstream = null;
                    }
                    if (((this._temporaryFileName != null) && (this._name != null)) && (this._writestream != null))
                    {
                        this._writestream.Dispose();
                        this._writestream = null;
                    }
                }
                this._disposed = true;
            }
        }

        public void Extract(String fileName)
        {
            ZipEntry e = this[fileName];
            e.Extract();
        }

        public void Extract(String fileName, bool wantOverwrite)
        {
            ZipEntry e = this[fileName];
            e.Extract(wantOverwrite);
        }

        public void Extract(String fileName, Stream outputStream)
        {
            if (!((outputStream != null) && outputStream.CanWrite))
            {
                throw new ZipException("Cannot extract.", new ArgumentException("The OutputStream must be a writable stream.", "outputStream"));
            }
            if (String.IsNullOrEmpty(fileName))
            {
                throw new ZipException("Cannot extract.", new ArgumentException("The file name must be neither null nor empty.", "fileName"));
            }
            ZipEntry e = this[fileName];
            e.Extract(outputStream);
        }

        public void Extract(String fileName, String directoryName)
        {
            ZipEntry e = this[fileName];
            e.Extract(directoryName);
        }

        public void Extract(String fileName, String directoryName, bool wantOverwrite)
        {
            ZipEntry e = this[fileName];
            e.Extract(directoryName, wantOverwrite);
        }

        public void ExtractAll(String path)
        {
            this.ExtractAll(path, false);
        }

        public void ExtractAll(String path, bool wantOverwrite)
        {
            bool header = this.Verbose;
            int n = 0;
            foreach (ZipEntry e in this._entries)
            {
                if (header)
                {
                    this.StatusMessageTextWriter.WriteLine("\n{1,-22} {2,-8} {3,4}   {4,-8}  {0}", new Object[] { "Name", "Modified", "Size", "Ratio", "Packed" });
                    this.StatusMessageTextWriter.WriteLine(new String('-', 0x48));
                    header = false;
                }
                if (this.Verbose)
                {
                    this.StatusMessageTextWriter.WriteLine("{1,-22} {2,-8} {3,4:F0}%   {4,-8} {0}", new Object[] { e.FileName, e.LastModified.ToString("yyyy-MM-dd HH:mm:ss"), e.UncompressedSize, e.CompressionRatio, e.CompressedSize });
                    if (!String.IsNullOrEmpty(e.Comment))
                    {
                        this.StatusMessageTextWriter.WriteLine("  Comment: {0}", e.Comment);
                    }
                }
                e.Extract(path, wantOverwrite);
                n++;
            }
        }

        ~ZipFile()
        {
            this.Dispose(false);
        }

        internal static String GenerateUniquePathname(String extension, String ContainingDirectory)
        {
            String candidate = null;
            String AppName = Assembly.GetExecutingAssembly().GetName().Name;
            String parentDir = (ContainingDirectory == null) ? Environment.GetEnvironmentVariable("TEMP") : ContainingDirectory;
            if (parentDir == null)
            {
                return null;
            }
            int index = 0;
            do
            {
                index++;
                String Name = String.Format("{0}-{1}-{2}.{3}", new Object[] { AppName, DateTime.Now.ToString("yyyyMMMdd-HHmmss"), index, extension });
                candidate = Path.Combine(parentDir, Name);
            }
            while (File.Exists(candidate) || Directory.Exists(candidate));
            return candidate;
        }

        public IEnumerator<ZipEntry> GetEnumerator()
        {
            foreach (var e in _entries) yield return e;
        }

        private void InitFile(String zipFileName, TextWriter statusMessageWriter)
        {
            this._name = zipFileName;
            this._StatusMessageTextWriter = statusMessageWriter;
            this._contentsChanged = true;
            if (File.Exists(this._name))
            {
                ReadIntoInstance(this);
                this._fileAlreadyExists = true;
            }
            else
            {
                InitializeEntries(true);
            }
        }

        private void InsureUniqueEntry(ZipEntry ze1)
        {
            var normalizedName = ZipSharedUtilities.TrimVolumeAndSwapSlashes(ze1.FileName);
            if (_entriesIndex.ContainsKey(normalizedName))
            {
                throw new ArgumentException(String.Format("The entry '{0}' already exists in the zip archive.", ze1.FileName));
            }
        }

        public static bool IsZipFile(String fileName)
        {
            bool result = false;
            try
            {
                using (Read(fileName, null, Encoding.GetEncoding("IBM437")))
                {
                }
                result = true;
            }
            catch (ZipException)
            {
            }
            return result;
        }

        public static ZipFile Read(byte[] buffer)
        {
            return Read(buffer, null, DefaultEncoding);
        }

        public static ZipFile Read(Stream zipStream)
        {
            return Read(zipStream, null, DefaultEncoding);
        }

        public static ZipFile Read(String zipFileName)
        {
            return Read(zipFileName, null, DefaultEncoding);
        }

        public static ZipFile Read(Stream zipStream, TextWriter statusMessageWriter)
        {
            return Read(zipStream, statusMessageWriter, DefaultEncoding);
        }

        public static ZipFile Read(Stream zipStream, Encoding encoding)
        {
            return Read(zipStream, null, encoding);
        }

        public static ZipFile Read(byte[] buffer, TextWriter statusMessageWriter)
        {
            return Read(buffer, statusMessageWriter, DefaultEncoding);
        }

        public static ZipFile Read(String zipFileName, TextWriter statusMessageWriter)
        {
            return Read(zipFileName, statusMessageWriter, DefaultEncoding);
        }

        public static ZipFile Read(String zipFileName, Encoding encoding)
        {
            return Read(zipFileName, null, encoding);
        }

        public static ZipFile Read(Stream zipStream, TextWriter statusMessageWriter, Encoding encoding)
        {
            if (zipStream == null)
            {
                throw new ZipException("Cannot read.", new ArgumentException("The stream must be non-null", "zipStream"));
            }
            ZipFile zf = new ZipFile {
                _encoding = encoding,
                _StatusMessageTextWriter = statusMessageWriter,
                _readstream = zipStream,
                _ReadStreamIsOurs = false
            };
            ReadIntoInstance(zf);
            return zf;
        }

        public static ZipFile Read(String zipFileName, TextWriter statusMessageWriter, Encoding encoding)
        {
            ZipFile zf = new ZipFile {
                Encoding = encoding,
                _StatusMessageTextWriter = statusMessageWriter,
                _name = zipFileName
            };
            try
            {
                ReadIntoInstance(zf);
                zf._fileAlreadyExists = true;
            }
            catch (Exception e1)
            {
                throw new ZipException(String.Format("{0} is not a valid zip file", zipFileName), e1);
            }
            return zf;
        }

        public static ZipFile Read(byte[] buffer, TextWriter statusMessageWriter, Encoding encoding)
        {
            ZipFile zf = new ZipFile {
                _StatusMessageTextWriter = statusMessageWriter,
                _encoding = encoding,
                _readstream = new MemoryStream(buffer),
                _ReadStreamIsOurs = true
            };
            ReadIntoInstance(zf);
            return zf;
        }

        private static void ReadCentralDirectoryFooter(ZipFile zf)
        {
            // no seek - stream integrity should be protected by the caller

            Stream s = zf.ReadStream;
            int signature = ZipSharedUtilities.ReadSignature(s);
            if (signature != 0x6054b50L)
            {
                s.Seek(-4L, SeekOrigin.Current);
                throw new BadReadException(String.Format("  ZipFile::Read(): Bad signature ({0:X8}) at position 0x{1:X8}", signature, s.Position));
            }
            byte[] block = new byte[0x10];
            zf.ReadStream.Read(block, 0, block.Length);
            ReadZipFileComment(zf);
        }

        private static void ReadIntoInstance(ZipFile zf)
        {
            lock (zf)
            {
                try
                {
                    ZipEntry e;
                    ZipDirEntry de;
                    zf.InitializeEntries(false);
                    if (zf.Verbose)
                    {
                        if (zf.Name == null)
                        {
                            zf.StatusMessageTextWriter.WriteLine("Reading zip from stream...");
                        }
                        else
                        {
                            zf.StatusMessageTextWriter.WriteLine("Reading zip {0}...", zf.Name);
                        }
                    }
                    while ((e = ZipEntry.Read(zf.ReadStream, zf.Encoding)) != null)
                    {
                        if (zf.Verbose)
                        {
                            zf.StatusMessageTextWriter.WriteLine("  {0}", e.FileName);
                        }
                        zf._entries.Add(e);
                    }

                    zf.IndexEntriesAndTrackChangesSinceNow();
                    var index = zf._entriesIndex;

                    zf._direntries = new List<ZipDirEntry>();
                    while ((de = ZipDirEntry.Read(zf.ReadStream, zf.Encoding)) != null)
                    {
                        zf._direntries.Add(de);

                        if (index.ContainsKey(de.FileName))
                        {
                            var e1 = index[de.FileName];

                            e1.Comment = de.Comment;
                            if (de.IsDirectory)
                            {
                                e1.MarkAsDirectory();
                            }
                        }
                    }
                    ReadCentralDirectoryFooter(zf);
                    if (!(!zf.Verbose || String.IsNullOrEmpty(zf.Comment)))
                    {
                        zf.StatusMessageTextWriter.WriteLine("Zip file Comment: {0}", zf.Comment);
                    }
                }
                catch (Exception e1)
                {
                    if (zf._ReadStreamIsOurs && (zf._readstream != null))
                    {
                        try
                        {
                            zf._readstream.Close();
                            zf._readstream.Dispose();
                            zf._readstream = null;
                        }
                        finally
                        {
                        }
                    }
                    throw new ZipException("Exception while reading", e1);
                }
            }
        }

        private static void ReadZipFileComment(ZipFile zf)
        {
            // no seek - stream integrity should be protected by the caller

            byte[] block = new byte[2];
            zf.ReadStream.Read(block, 0, block.Length);
            short commentLength = (short)(block[0] + (block[1] * 0x100));
            if (commentLength > 0)
            {
                block = new byte[commentLength];
                zf.ReadStream.Read(block, 0, block.Length);
                if (ZipSharedUtilities.HighBytes(block) && (zf._encoding == Encoding.GetEncoding("ibm437")))
                {
                    zf._encoding = Encoding.UTF8;
                }
                zf.Comment = ZipSharedUtilities.StringFromBuffer(block, block.Length, zf._encoding);
            }
        }

        public void RemoveEntry(ZipEntry entry)
        {
            if (!this._entries.Contains(entry))
            {
                throw new ArgumentException("The entry you specified does not exist in the zip archive.");
            }
            this._entries.Remove(entry);
            if (this._direntries != null)
            {
                bool FoundAndRemovedDirEntry = false;
                foreach (ZipDirEntry de1 in this._direntries)
                {
                    // todo. performance!
                    if (entry.FileName == de1.FileName)
                    {
                        this._direntries.Remove(de1);
                        FoundAndRemovedDirEntry = true;
                        break;
                    }
                }
                if (!FoundAndRemovedDirEntry)
                {
                    throw new BadStateException("The entry to be removed was not found in the directory.");
                }
            }
            this._contentsChanged = true;
        }

        public void RemoveEntry(String fileName)
        {
            String modifiedName = ZipEntry.NameInArchive(fileName, null);
            ZipEntry e = this[modifiedName];
            if (e == null)
            {
                throw new ArgumentException("The entry you specified was not found in the zip archive.");
            }
            this.RemoveEntry(e);
        }

        private void RemoveTempFile()
        {
            try
            {
                if (File.Exists(this._temporaryFileName))
                {
                    File.Delete(this._temporaryFileName);
                }
            }
            catch (Exception ex1)
            {
                this.StatusMessageTextWriter.WriteLine("ZipFile::Save: could not delete temp file: {0}.", ex1.Message);
            }
        }

        internal void Reset()
        {
            // todo. all this stuff just to copy the metadata???
            if (this._JustSaved)
            {
                using(var zipFromHdd = new ZipFile{_name = this._name, Encoding = this.Encoding})
                {
                    ReadIntoInstance(zipFromHdd);
                    foreach (var e1 in zipFromHdd)
                    {
                        var e2 = this._entriesIndex.GetOrDefault(e1.FileName);
                        if (e2 != null)
                        {
                            e2.CopyMetaData(e1);
                        }
                    }

                    this._JustSaved = false;
                }
            }
        }

        public void Save()
        {
            try
            {
                this._saveOperationCanceled = false;
                if (this.WriteStream == null)
                {
                    throw new BadStateException("You haven't specified where to save the zip.");
                }
                if (this._contentsChanged)
                {
                    if (this.Verbose)
                    {
                        this.StatusMessageTextWriter.WriteLine("Saving....");
                    }
                    if (this._entries.Count >= 0xffff)
                    {
                        throw new ZipException("The number of entries is 0xFFFF or greater. DotNetZip does not currently support the ZIP64 format.");
                    }
                    int n = 0;
                    foreach (ZipEntry e in this._entries)
                    {
                        e.Write(this.WriteStream);
                        e.ZipFile = this;
                        n++;
                        if (this._saveOperationCanceled)
                        {
                            break;
                        }
                    }
                    if (!this._saveOperationCanceled)
                    {
                        this.WriteCentralDirectoryStructure(this.WriteStream);
                        if ((this._temporaryFileName != null) && (this._name != null))
                        {
                            this.WriteStream.Close();
                            this.WriteStream.Dispose();
                            this.WriteStream = null;
                            if (this._saveOperationCanceled)
                            {
                                return;
                            }
                            if (this._fileAlreadyExists && (this._readstream != null))
                            {
                                this._readstream.Close();
                                this._readstream = null;
                            }
                            if (this._fileAlreadyExists)
                            {
                                File.Delete(this._name);
                                File.Move(this._temporaryFileName, this._name);
                            }
                            else
                            {
                                File.Move(this._temporaryFileName, this._name);
                            }
                            this._fileAlreadyExists = true;
                        }
                        this._JustSaved = true;
                    }
                }
            }
            finally
            {
                this.CleanupAfterSaveOperation();
            }
        }

        public void Save(String zipFileName)
        {
            if (this._name == null)
            {
                this._writestream = null;
            }
            this._name = zipFileName;
            if (Directory.Exists(this._name))
            {
                throw new ZipException("Bad Directory", new ArgumentException("That name specifies an existing directory. Please specify a filename.", "zipFileName"));
            }
            this._contentsChanged = true;
            this._fileAlreadyExists = File.Exists(this._name);
            this.Save();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void UpdateDirectory(String directoryName)
        {
            this.UpdateDirectory(directoryName, null);
        }

        public void UpdateDirectory(String directoryName, String directoryPathInArchive)
        {
            this.AddOrUpdateDirectoryImpl(directoryName, directoryPathInArchive, AddOrUpdateAction.AddOrUpdate);
        }

        public ZipEntry UpdateFile(String fileName)
        {
            return this.UpdateFile(fileName, null);
        }

        public ZipEntry UpdateFile(String fileName, String directoryPathInArchive)
        {
            String key = ZipEntry.NameInArchive(fileName, directoryPathInArchive);
            if (this[key] != null)
            {
                this.RemoveEntry(key);
            }
            return this.AddFile(fileName, directoryPathInArchive);
        }

        public ZipEntry UpdateFileStream(String fileName, String directoryPathInArchive, Stream stream)
        {
            String key = ZipEntry.NameInArchive(fileName, directoryPathInArchive);
            if (this[key] != null)
            {
                this.RemoveEntry(key);
            }
            return this.AddFileStream(fileName, directoryPathInArchive, stream);
        }

        public void UpdateItem(String itemName)
        {
            this.UpdateItem(itemName, null);
        }

        public void UpdateItem(String itemName, String directoryPathInArchive)
        {
            if (File.Exists(itemName))
            {
                this.UpdateFile(itemName, directoryPathInArchive);
            }
            else
            {
                if (!Directory.Exists(itemName))
                {
                    throw new FileNotFoundException(String.Format("That file or directory ({0}) does not exist!", itemName));
                }
                this.UpdateDirectory(itemName, directoryPathInArchive);
            }
        }

        private void WriteCentralDirectoryFooter(Stream s, long StartOfCentralDirectory, long EndOfCentralDirectory)
        {
            int bufferLength = 0x16;
            byte[] block = null;
            short commentLength = 0;
            if ((this.Comment != null) && (this.Comment.Length != 0))
            {
                block = this.Comment.ToByteArray(this.Encoding);
                commentLength = (short) block.Length;
            }
            bufferLength += commentLength;
            byte[] bytes = new byte[bufferLength];
            int i = 0;
            bytes[i++] = 80;
            bytes[i++] = 0x4b;
            bytes[i++] = 5;
            bytes[i++] = 6;
            bytes[i++] = 0;
            bytes[i++] = 0;
            bytes[i++] = 0;
            bytes[i++] = 0;
            bytes[i++] = (byte) (this._entries.Count & 0xff);
            bytes[i++] = (byte) ((this._entries.Count & 0xff00) >> 8);
            bytes[i++] = (byte) (this._entries.Count & 0xff);
            bytes[i++] = (byte) ((this._entries.Count & 0xff00) >> 8);
            int SizeOfCentralDirectory = (int) (EndOfCentralDirectory - StartOfCentralDirectory);
            bytes[i++] = (byte) (SizeOfCentralDirectory & 0xff);
            bytes[i++] = (byte) ((SizeOfCentralDirectory & 0xff00) >> 8);
            bytes[i++] = (byte) ((SizeOfCentralDirectory & 0xff0000) >> 0x10);
            bytes[i++] = (byte) ((SizeOfCentralDirectory & 0xff000000L) >> 0x18);
            int StartOffset = (int) StartOfCentralDirectory;
            bytes[i++] = (byte) (StartOffset & 0xff);
            bytes[i++] = (byte) ((StartOffset & 0xff00) >> 8);
            bytes[i++] = (byte) ((StartOffset & 0xff0000) >> 0x10);
            bytes[i++] = (byte) ((StartOffset & 0xff000000L) >> 0x18);
            if ((this.Comment == null) || (this.Comment.Length == 0))
            {
                bytes[i++] = 0;
                bytes[i++] = 0;
            }
            else
            {
                if (((commentLength + i) + 2) > bytes.Length)
                {
                    commentLength = (short) ((bytes.Length - i) - 2);
                }
                bytes[i++] = (byte) (commentLength & 0xff);
                bytes[i++] = (byte) ((commentLength & 0xff00) >> 8);
                if (commentLength != 0)
                {
                    int j = 0;
                    j = 0;
                    while ((j < commentLength) && ((i + j) < bytes.Length))
                    {
                        bytes[i + j] = block[j];
                        j++;
                    }
                    i += j;
                }
            }
            s.Write(bytes, 0, i);
        }

        private void WriteCentralDirectoryStructure(Stream s)
        {
            CountingStream output = s as CountingStream;
            long Start = (output != null) ? ((long) output.BytesWritten) : s.Position;
            foreach (ZipEntry e in this._entries)
            {
                e.WriteCentralDirectoryEntry(s);
            }
            long Finish = (output != null) ? ((long) output.BytesWritten) : s.Position;
            this.WriteCentralDirectoryFooter(s, Start, Finish);
        }

        // Properties
        public bool CaseSensitiveRetrieval
        {
            get
            {
                return this._CaseSensitiveRetrieval;
            }
            set
            {
                this._CaseSensitiveRetrieval = value;
            }
        }

        public String Comment
        {
            get
            {
                return this._Comment;
            }
            set
            {
                this._Comment = value;
                this._contentsChanged = true;
            }
        }

        public int Count
        {
            get
            {
                return this._entries.Count;
            }
        }

        public Encoding Encoding
        {
            get
            {
                return this._encoding;
            }
            set
            {
                this._encoding = value;
            }
        }

        public ReadOnlyCollection<ZipEntry> Entries
        {
            get
            {
                return this._entries.AsReadOnly();
            }
        }

        public ReadOnlyCollection<String> EntryFileNames
        {
            get
            {
                return this._entries.ConvertAll(e => e.FileName).AsReadOnly();
            }
        }

        public bool ForceNoCompression
        {
            get
            {
                return this._ForceNoCompression;
            }
            set
            {
                this._ForceNoCompression = value;
            }
        }

        public ZipEntry this[int ix]
        {
            get
            {
                return this._entries[ix];
            }
            set
            {
                if (value != null)
                {
                    throw new ArgumentException("You may not set this to a non-null ZipEntry value.");
                }
                this.RemoveEntry(this._entries[ix]);
            }
        }

        public ZipEntry this[String fileName]
        {
            get
            {
                // todo. performance
                foreach (ZipEntry e in this._entries)
                {
                    if (this.CaseSensitiveRetrieval)
                    {
                        if (e.FileName == fileName)
                        {
                            return e;
                        }
                        if (fileName.Replace(@"\", "/") == e.FileName)
                        {
                            return e;
                        }
                        if (e.FileName.Replace(@"\", "/") == fileName)
                        {
                            return e;
                        }
                    }
                    else
                    {
                        if (String.Compare(e.FileName, fileName, StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            return e;
                        }
                        if (String.Compare(fileName.Replace(@"\", "/"), e.FileName, StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            return e;
                        }
                        if (String.Compare(e.FileName.Replace(@"\", "/"), fileName, StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            return e;
                        }
                    }
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    throw new ArgumentException("You may not set this to a non-null ZipEntry value.");
                }
                this.RemoveEntry(fileName);
            }
        }

        public static Version LibraryVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        public String Name
        {
            get
            {
                return this._name;
            }
        }

        internal Stream ReadStream
        {
            get
            {
                if ((this._readstream == null) && (this._name != null))
                {
                    try
                    {
                        this._readstream = File.OpenRead(this._name);
                        this._ReadStreamIsOurs = true;
                    }
                    catch (IOException ioe)
                    {
                        throw new ZipException("Error opening the file", ioe);
                    }
                }
                return this._readstream;
            }
        }

        public TextWriter StatusMessageTextWriter
        {
            get
            {
                return this._StatusMessageTextWriter;
            }
            set
            {
                this._StatusMessageTextWriter = value;
            }
        }

        public String TempFileFolder
        {
            get
            {
                if (this._TempFileFolder == null)
                {
                    if (Environment.GetEnvironmentVariable("TEMP") != null)
                    {
                        this._TempFileFolder = Environment.GetEnvironmentVariable("TEMP");
                    }
                    else
                    {
                        this._TempFileFolder = ".";
                    }
                }
                return this._TempFileFolder;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentException("You may not set the TempFileFolder to a null value.");
                }
                if (!Directory.Exists(value))
                {
                    throw new FileNotFoundException(String.Format("That directory ({0}) does not exist.", value));
                }
                this._TempFileFolder = value;
            }
        }

        public bool TrimVolumeFromFullyQualifiedPaths
        {
            get
            {
                return this._TrimVolumeFromFullyQualifiedPaths;
            }
            set
            {
                this._TrimVolumeFromFullyQualifiedPaths = value;
            }
        }

        public bool UseUnicode
        {
            get
            {
                return (this._encoding == Encoding.GetEncoding("UTF-8"));
            }
            set
            {
                this._encoding = value ? Encoding.GetEncoding("UTF-8") : DefaultEncoding;
            }
        }

        private bool Verbose
        {
            get
            {
                return (this._StatusMessageTextWriter != null);
            }
        }

        private Stream WriteStream
        {
            get
            {
                if ((this._writestream == null) && (this._name != null))
                {
                    this._temporaryFileName = (this.TempFileFolder != ".") ? Path.Combine(this.TempFileFolder, Path.GetRandomFileName()) : Path.GetRandomFileName();
                    this._writestream = new FileStream(this._temporaryFileName, FileMode.CreateNew);
                }
                return this._writestream;
            }
            set
            {
                if (value != null)
                {
                    throw new ZipException("Whoa!", new ArgumentException("Cannot set the stream to a non-null value.", "value"));
                }
                this._writestream = null;
            }
        }
    }
}
