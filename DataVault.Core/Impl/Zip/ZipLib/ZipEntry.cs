using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using DataVault.Core.Helpers;
using DataVault.Core.Impl.Zip.ZipLib.Exceptions;
using DataVault.Core.Impl.Zip.ZipLib.Streams;

namespace DataVault.Core.Impl.Zip.ZipLib
{
    internal class ZipEntry
    {
        // Fields
        private long __FileDataPosition;
        private Stream _archiveStream;
        private short _BitField;
        private String _Comment;
        private byte[] _CommentBytes;
        private int _CompressedFileDataSize;
        private int _CompressedSize;
        private short _CompressionMethod;
        private int _Crc32;
        private Encoding _encoding = Encoding.GetEncoding("IBM437");
        private byte[] _EntryHeader;
        private byte[] _Extra;
        private String _FileNameInArchive;
        private bool _ForceNoCompression;
        private bool _IsDirectory;
        private DateTime _LastModified;
        private int _LengthOfHeader;
        private String _LocalFileName;
        private bool _OverwriteOnExtract;
        private int _RelativeOffsetOfHeader;
        internal EntrySource _Source = EntrySource.None;
        private Stream _sourceStream;
        private int _TimeBlob;
        private int _TotalEntrySize;
        private bool _TrimVolumeFromFullyQualifiedPaths = true;
        private int _UncompressedSize;
        private short _VersionNeeded;
        private ZipFile _zipfile;
        private const int READBLOCK_SIZE = 0x2200;

        // Methods
        private ZipEntry()
        {
        }

        private void _CheckRead(int nbytes)
        {
            if (nbytes == 0)
            {
                throw new BadReadException(String.Format("bad read of entry {0} from compressed archive.", this.FileName));
            }
        }

        private void _EmitOne(Stream outstream)
        {
            this._WriteFileData(outstream);
            this._TotalEntrySize = this._LengthOfHeader + this._CompressedSize;
        }

        private int _ExtractOne(Stream output)
        {
            lock (_streamLock)
            {
                Stream input = this.ArchiveStream;
                input.Seek(this.__FileDataPosition, SeekOrigin.Begin);
                byte[] bytes = new byte[0x2200];
                int LeftToRead = (this.CompressionMethod == 8) ? this.UncompressedSize : this._CompressedFileDataSize;
                Stream input3 = (this.CompressionMethod == 8) ? new DeflateStream(input, CompressionMode.Decompress, true) : input;
                using (CrcCalculatorStream s1 = new CrcCalculatorStream(input3))
                {
                    while (LeftToRead > 0)
                    {
                        int len = (LeftToRead > bytes.Length) ? bytes.Length : LeftToRead;
                        int n = s1.Read(bytes, 0, len);
                        this._CheckRead(n);
                        output.Write(bytes, 0, n);
                        LeftToRead -= n;
                    }
                    return s1.Crc32;
                }
            }
        }

        private void _WriteFileData(Stream s)
        {
            Stream input = null;
            CrcCalculatorStream input1 = null;
            CountingStream counter = null;
            try
            {
                this.__FileDataPosition = s.Position;
            }
            catch
            {
            }
            try
            {
                if (this._sourceStream != null)
                {
                    this._sourceStream.Position = 0L;
                    input = this._sourceStream;
                }
                else
                {
                    input = File.OpenRead(this.LocalFileName);
                }
                input1 = new CrcCalculatorStream(input);
                counter = new CountingStream(s);
                Stream output1 = counter;
                Stream output2 = null;
                bool mustCloseDeflateStream = false;
                if (this.CompressionMethod == 8)
                {
                    output2 = new DeflateStream(output1, CompressionMode.Compress, true);
                    mustCloseDeflateStream = true;
                }
                else
                {
                    output2 = output1;
                }
                byte[] buffer = new byte[0x2200];
                for (int n = input1.Read(buffer, 0, 0x2200); n > 0; n = input1.Read(buffer, 0, 0x2200))
                {
                    output2.Write(buffer, 0, n);
                }
                if (mustCloseDeflateStream)
                {
                    output2.Close();
                }
            }
            finally
            {
                if ((this._sourceStream == null) && (input != null))
                {
                    input.Close();
                    input.Dispose();
                }
            }
            this._UncompressedSize = input1.TotalBytesSlurped;
            this._CompressedSize = counter.BytesWritten;
            this._Crc32 = input1.Crc32;
            int i = 8;
            this._EntryHeader[i++] = (byte) (this.CompressionMethod & 0xff);
            this._EntryHeader[i++] = (byte) ((this.CompressionMethod & 0xff00) >> 8);
            i = 14;
            this._EntryHeader[i++] = (byte) (this._Crc32 & 0xff);
            this._EntryHeader[i++] = (byte) ((this._Crc32 & 0xff00) >> 8);
            this._EntryHeader[i++] = (byte) ((this._Crc32 & 0xff0000) >> 0x10);
            this._EntryHeader[i++] = (byte) ((this._Crc32 & 0xff000000L) >> 0x18);
            this._EntryHeader[i++] = (byte) (this._CompressedSize & 0xff);
            this._EntryHeader[i++] = (byte) ((this._CompressedSize & 0xff00) >> 8);
            this._EntryHeader[i++] = (byte) ((this._CompressedSize & 0xff0000) >> 0x10);
            this._EntryHeader[i++] = (byte) ((this._CompressedSize & 0xff000000L) >> 0x18);
            this._EntryHeader[i++] = (byte) (this._UncompressedSize & 0xff);
            this._EntryHeader[i++] = (byte) ((this._UncompressedSize & 0xff00) >> 8);
            this._EntryHeader[i++] = (byte) ((this._UncompressedSize & 0xff0000) >> 0x10);
            this._EntryHeader[i++] = (byte) ((this._UncompressedSize & 0xff000000L) >> 0x18);
            if (s.CanSeek)
            {
                s.Seek((long) this._RelativeOffsetOfHeader, SeekOrigin.Begin);
                s.Write(this._EntryHeader, 0, this._EntryHeader.Length);
                CountingStream s1 = s as CountingStream;
                if (s1 != null)
                {
                    s1.Adjust(this._EntryHeader.Length);
                }
                s.Seek((long) this._CompressedSize, SeekOrigin.Current);
            }
            else
            {
                if ((this._BitField & 8) != 8)
                {
                    throw new ZipException("Logic error.");
                }
                byte[] Descriptor = new byte[0x10];
                i = 0;
                int sig = 0x8074b50;
                Descriptor[i++] = (byte) (sig & 0xff);
                Descriptor[i++] = (byte) ((sig & 0xff00) >> 8);
                Descriptor[i++] = (byte) ((sig & 0xff0000) >> 0x10);
                Descriptor[i++] = (byte) ((sig & 0xff000000L) >> 0x18);
                Descriptor[i++] = (byte) (this._Crc32 & 0xff);
                Descriptor[i++] = (byte) ((this._Crc32 & 0xff00) >> 8);
                Descriptor[i++] = (byte) ((this._Crc32 & 0xff0000) >> 0x10);
                Descriptor[i++] = (byte) ((this._Crc32 & 0xff000000L) >> 0x18);
                Descriptor[i++] = (byte) (this._CompressedSize & 0xff);
                Descriptor[i++] = (byte) ((this._CompressedSize & 0xff00) >> 8);
                Descriptor[i++] = (byte) ((this._CompressedSize & 0xff0000) >> 0x10);
                Descriptor[i++] = (byte) ((this._CompressedSize & 0xff000000L) >> 0x18);
                Descriptor[i++] = (byte) (this._UncompressedSize & 0xff);
                Descriptor[i++] = (byte) ((this._UncompressedSize & 0xff00) >> 8);
                Descriptor[i++] = (byte) ((this._UncompressedSize & 0xff0000) >> 0x10);
                Descriptor[i++] = (byte) ((this._UncompressedSize & 0xff000000L) >> 0x18);
                s.Write(Descriptor, 0, Descriptor.Length);
            }
        }

        internal void CopyMetaData(ZipEntry source)
        {
            this.__FileDataPosition = source.__FileDataPosition;
            this.CompressionMethod = source.CompressionMethod;
            this._CompressedFileDataSize = source._CompressedFileDataSize;
            this._UncompressedSize = source._UncompressedSize;
            this._BitField = source._BitField;
            this._LastModified = source._LastModified;
        }

        private void CopyThroughOneEntry(Stream outstream)
        {
            lock (_streamLock)
            {
                byte[] bytes = new byte[0x2200];
                Stream input = this.ArchiveStream;
                input.Seek((long)this._RelativeOffsetOfHeader, SeekOrigin.Begin);
                this._EntryHeader = new byte[this._LengthOfHeader];
                int n = input.Read(this._EntryHeader, 0, this._EntryHeader.Length);
                this._CheckRead(n);
                input.Seek((long)this._RelativeOffsetOfHeader, SeekOrigin.Begin);
                CountingStream counter = outstream as CountingStream;
                this._RelativeOffsetOfHeader = (counter != null) ? counter.BytesWritten : ((int)outstream.Position);
                for (int Remaining = this._TotalEntrySize; Remaining > 0; Remaining -= n)
                {
                    int len = (Remaining > bytes.Length) ? bytes.Length : Remaining;
                    n = input.Read(bytes, 0, len);
                    this._CheckRead(n);
                    outstream.Write(bytes, 0, n);
                }
            }
        }

        internal static ZipEntry Create(String filename, String nameInArchive)
        {
            return Create(filename, nameInArchive, null);
        }

        internal static ZipEntry Create(String filename, String nameInArchive, Stream stream)
        {
            if (String.IsNullOrEmpty(filename))
            {
                throw new ZipException("The entry name must be non-null and non-empty.");
            }
            ZipEntry entry = new ZipEntry();
            if (stream != null)
            {
                entry._sourceStream = stream;
                entry._LastModified = DateTime.Now;
            }
            else
            {
                entry._LastModified = (File.Exists(filename) || Directory.Exists(filename)) ? ZipSharedUtilities.RoundToEvenSecond(File.GetLastWriteTime(filename)) : DateTime.Now;
                if (!(entry._LastModified.IsDaylightSavingTime() || !DateTime.Now.IsDaylightSavingTime()))
                {
                    entry._LastModified += new TimeSpan(1, 0, 0);
                }
                if (!(!entry._LastModified.IsDaylightSavingTime() || DateTime.Now.IsDaylightSavingTime()))
                {
                    entry._LastModified -= new TimeSpan(1, 0, 0);
                }
            }
            entry._LocalFileName = filename;
            entry._FileNameInArchive = nameInArchive.Replace('\\', '/');
            return entry;
        }

        private bool DefaultWantCompression()
        {
            if (this._LocalFileName != null)
            {
                return this.SeemsCompressible(this._LocalFileName);
            }
            if (this._FileNameInArchive != null)
            {
                return this.SeemsCompressible(this._FileNameInArchive);
            }
            return true;
        }

        public void Extract()
        {
            this.InternalExtract(".", null);
        }

        public void Extract(bool overwrite)
        {
            this.OverwriteOnExtract = overwrite;
            this.InternalExtract(".", null);
        }

        public void Extract(Stream stream)
        {
            this.InternalExtract(null, stream);
        }

        public void Extract(String baseDirectory)
        {
            this.InternalExtract(baseDirectory, null);
        }

        public void Extract(String baseDirectory, bool overwrite)
        {
            this.OverwriteOnExtract = overwrite;
            this.InternalExtract(baseDirectory, null);
        }

        private void FigureCompressionMethodForWriting(int cycle)
        {
            if (cycle > 1)
            {
                this._CompressionMethod = 0;
            }
            else if (this.IsDirectory)
            {
                this._CompressionMethod = 0;
            }
            else if (this.__FileDataPosition == 0L)
            {
                long fileLength = 0L;
                if (this._sourceStream != null)
                {
                    fileLength = this._sourceStream.Length;
                }
                else
                {
                    FileInfo fi = new FileInfo(this.LocalFileName);
                    fileLength = fi.Length;
                }
                if (fileLength == 0L)
                {
                    this._CompressionMethod = 0;
                }
                else if (this._ForceNoCompression)
                {
                    this._CompressionMethod = 0;
                }
                else
                {
                    this._CompressionMethod = this.DefaultWantCompression() ? ((short) 8) : ((short) 0);
                }
            }
        }

        private byte[] GetFileNameBytes()
        {
            String SlashFixed = this.FileName.Replace(@"\", "/");
            String result = null;
            if (((this.TrimVolumeFromFullyQualifiedPaths && (this.FileName.Length >= 3)) && (this.FileName[1] == ':')) && (SlashFixed[2] == '/'))
            {
                result = SlashFixed.Substring(3);
            }
            else if ((this.FileName.Length >= 4) && ((SlashFixed[0] == '/') && (SlashFixed[1] == '/')))
            {
                int n = SlashFixed.IndexOf('/', 2);
                if (n == -1)
                {
                    throw new ArgumentException("The path for that entry appears to be badly formatted");
                }
                result = SlashFixed.Substring(n + 1);
            }
            else if ((this.FileName.Length >= 3) && ((SlashFixed[0] == '.') && (SlashFixed[1] == '/')))
            {
                result = SlashFixed.Substring(2);
            }
            else
            {
                result = SlashFixed;
            }
            return result.ToByteArray(this._encoding);
        }

        private static void HandleUnexpectedDataDescriptor(ZipEntry entry)
        {
            // no seek - stream integrity should be protected by the caller

            Stream s = entry.ArchiveStream;
            if (ZipSharedUtilities.ReadInt(s) == entry._Crc32)
            {
                if (ZipSharedUtilities.ReadInt(s) == entry._CompressedSize)
                {
                    if (ZipSharedUtilities.ReadInt(s) != entry._UncompressedSize)
                    {
                        s.Seek(-12L, SeekOrigin.Current);
                    }
                }
                else
                {
                    s.Seek(-8L, SeekOrigin.Current);
                }
            }
            else
            {
                s.Seek(-4L, SeekOrigin.Current);
            }
        }

        private void InternalExtract(String baseDir, Stream outstream)
        {
            String TargetFile = null;
            Stream output = null;
            bool fileExistsBeforeExtraction = false;
            try
            {
                this.ValidateCompression();
                if (!this.ValidateOutput(baseDir, outstream, out TargetFile))
                {
                    if (TargetFile != null)
                    {
                        if (!Directory.Exists(Path.GetDirectoryName(TargetFile)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(TargetFile));
                        }
                        if (File.Exists(TargetFile))
                        {
                            fileExistsBeforeExtraction = true;
                            if (!this.OverwriteOnExtract)
                            {
                                throw new ZipException("The file already exists.");
                            }
                            File.Delete(TargetFile);
                        }
                        output = new FileStream(TargetFile, FileMode.CreateNew);
                    }
                    else
                    {
                        output = outstream;
                    }
                    int ActualCrc32 = this._ExtractOne(output);
                    if (ActualCrc32 != this._Crc32)
                    {
                        throw new BadCrcException("CRC error: the file being extracted appears to be corrupted. " + String.Format("Expected 0x{0:X8}, actual 0x{1:X8}", this._Crc32, ActualCrc32));
                    }
                    if (TargetFile != null)
                    {
                        output.Close();
                        DateTime AdjustedLastModified = this.LastModified;
                        if (DateTime.Now.IsDaylightSavingTime() && !this.LastModified.IsDaylightSavingTime())
                        {
                            AdjustedLastModified = this.LastModified - new TimeSpan(1, 0, 0);
                        }
                        File.SetLastWriteTime(TargetFile, AdjustedLastModified);
                    }
                }
            }
            catch
            {
                try
                {
                    if (TargetFile != null)
                    {
                        if (output != null)
                        {
                            output.Close();
                        }
                        if (File.Exists(TargetFile) && !(fileExistsBeforeExtraction && !this.OverwriteOnExtract))
                        {
                            File.Delete(TargetFile);
                        }
                    }
                }
                finally
                {
                }
                throw;
            }
        }

        private static bool IsNotValidSig(int signature)
        {
            return (signature != 0x4034b50);
        }

        internal void MarkAsDirectory()
        {
            this._IsDirectory = true;
            if (!this._FileNameInArchive.EndsWith("/"))
            {
                this._FileNameInArchive = this._FileNameInArchive + "/";
            }
        }

        internal static String NameInArchive(String filename, String directoryPathInArchive)
        {
            String result = null;
            if (directoryPathInArchive == null)
            {
                result = filename;
            }
            else if (String.IsNullOrEmpty(directoryPathInArchive))
            {
                result = Path.GetFileName(filename);
            }
            else
            {
                result = Path.Combine(directoryPathInArchive, Path.GetFileName(filename));
            }
            return ZipSharedUtilities.TrimVolumeAndSwapSlashes(result);
        }

        internal static ZipEntry Read(Stream s, Encoding defaultEncoding)
        {
            // no seek - stream integrity should be protected by the caller

            ZipEntry entry = new ZipEntry
            {
                _Source = EntrySource.Zipfile,
                _archiveStream = s
            };
            if (!ReadHeader(entry, defaultEncoding))
            {
                return null;
            }
            entry.__FileDataPosition = entry.ArchiveStream.Position;
            s.Seek((long) entry._CompressedFileDataSize, SeekOrigin.Current);
            if (!entry.FileName.EndsWith("/") && ((entry._BitField & 8) == 8))
            {
                s.Seek(0x10L, SeekOrigin.Current);
            }
            HandleUnexpectedDataDescriptor(entry);
            return entry;
        }

        private static bool ReadHeader(ZipEntry ze, Encoding defaultEncoding)
        {
            // no seek - stream integrity should be protected by the caller

            int bytesRead = 0;
            ze._RelativeOffsetOfHeader = (int) ze.ArchiveStream.Position;
            int signature = ZipSharedUtilities.ReadSignature(ze.ArchiveStream);
            bytesRead += 4;
            if (IsNotValidSig(signature))
            {
                ze.ArchiveStream.Seek(-4L, SeekOrigin.Current);
                if (ZipDirEntry.IsNotValidSig(signature) && (signature != 0x6054b50L))
                {
                    throw new BadReadException(String.Format("  ZipEntry::ReadHeader(): Bad signature (0x{0:X8}) at position  0x{1:X8}", signature, ze.ArchiveStream.Position));
                }
                return false;
            }
            byte[] block = new byte[0x1a];
            int n = ze.ArchiveStream.Read(block, 0, block.Length);
            if (n != block.Length)
            {
                return false;
            }
            bytesRead += n;
            int i = 0;
            ze._VersionNeeded = (short) (block[i++] + (block[i++] * 0x100));
            ze._BitField = (short) (block[i++] + (block[i++] * 0x100));
            ze._CompressionMethod = (short) (block[i++] + (block[i++] * 0x100));
            ze._TimeBlob = ((block[i++] + (block[i++] * 0x100)) + ((block[i++] * 0x100) * 0x100)) + (((block[i++] * 0x100) * 0x100) * 0x100);
            ze._LastModified = ZipSharedUtilities.PackedToDateTime(ze._TimeBlob);
            if ((ze._BitField & 8) != 8)
            {
                ze._Crc32 = ((block[i++] + (block[i++] * 0x100)) + ((block[i++] * 0x100) * 0x100)) + (((block[i++] * 0x100) * 0x100) * 0x100);
                ze._CompressedSize = ((block[i++] + (block[i++] * 0x100)) + ((block[i++] * 0x100) * 0x100)) + (((block[i++] * 0x100) * 0x100) * 0x100);
                ze._UncompressedSize = ((block[i++] + (block[i++] * 0x100)) + ((block[i++] * 0x100) * 0x100)) + (((block[i++] * 0x100) * 0x100) * 0x100);
            }
            else
            {
                i += 12;
            }
            short filenameLength = (short) (block[i++] + (block[i++] * 0x100));
            short extraFieldLength = (short) (block[i++] + (block[i++] * 0x100));
            block = new byte[filenameLength];
            n = ze.ArchiveStream.Read(block, 0, block.Length);
            bytesRead += n;

            var s1 = ZipSharedUtilities.StringFromBuffer(block, block.Length, Encoding.GetEncoding(1251));
            var s2 = ZipSharedUtilities.StringFromBuffer(block, block.Length, Encoding.ASCII);
            var s3 = ZipSharedUtilities.StringFromBuffer(block, block.Length, Encoding.UTF8);
            var s4 = ZipSharedUtilities.StringFromBuffer(block, block.Length, Encoding.Default);
            var s5 = ZipSharedUtilities.StringFromBuffer(block, block.Length, Encoding.GetEncoding(1252));

            if ((ze._BitField & 0x800) == 0x800)
            {
                ze._FileNameInArchive = ZipSharedUtilities.StringFromBuffer(block, block.Length, Encoding.UTF8);
                ze.UseUtf8Encoding = true;
            }
            else
            {
                ze._FileNameInArchive = ZipSharedUtilities.StringFromBuffer(block, block.Length, defaultEncoding);
                ze._encoding = defaultEncoding;
            }
            ze._LocalFileName = ze._FileNameInArchive;
            if (extraFieldLength > 0)
            {
                ze._Extra = new byte[extraFieldLength];
                n = ze.ArchiveStream.Read(ze._Extra, 0, ze._Extra.Length);
                bytesRead += n;
            }
            if (!ze.FileName.EndsWith("/") && ((ze._BitField & 8) == 8))
            {
                long posn = ze.ArchiveStream.Position;
                bool wantMore = true;
                long SizeOfDataRead = 0L;
                int tries = 0;
                while (wantMore)
                {
                    tries++;
                    long d = ZipSharedUtilities.FindSignature(ze.ArchiveStream, 0x8074b50);
                    if (d == -1L)
                    {
                        return false;
                    }
                    SizeOfDataRead += d;
                    block = new byte[12];
                    n = ze.ArchiveStream.Read(block, 0, block.Length);
                    if (n != 12)
                    {
                        return false;
                    }
                    bytesRead += n;
                    i = 0;
                    ze._Crc32 = ((block[i++] + (block[i++] * 0x100)) + ((block[i++] * 0x100) * 0x100)) + (((block[i++] * 0x100) * 0x100) * 0x100);
                    ze._CompressedSize = ((block[i++] + (block[i++] * 0x100)) + ((block[i++] * 0x100) * 0x100)) + (((block[i++] * 0x100) * 0x100) * 0x100);
                    ze._UncompressedSize = ((block[i++] + (block[i++] * 0x100)) + ((block[i++] * 0x100) * 0x100)) + (((block[i++] * 0x100) * 0x100) * 0x100);
                    if (SizeOfDataRead != ze._CompressedSize)
                    {
                        ze.ArchiveStream.Seek(-12L, SeekOrigin.Current);
                        SizeOfDataRead += 4L;
                    }
                }
                ze.ArchiveStream.Seek(posn, SeekOrigin.Begin);
            }
            ze._CompressedFileDataSize = ze._CompressedSize;
            if ((ze._BitField & 1) == 1)
            {
                throw new NotSupportedException("Encryption is not supported");
            }
            ze._TotalEntrySize = bytesRead + ze._CompressedFileDataSize;
            ze._LengthOfHeader = bytesRead;
            return true;
        }

        private bool SeemsCompressible(String filename)
        {
            String re = @"^(?i)(.+)\.(mp3|png|docx|xlsx|zip)$";
            if (Regex.IsMatch(filename, re))
            {
                return false;
            }
            return true;
        }

        private void ValidateCompression()
        {
            if ((this.CompressionMethod != 0) && (this.CompressionMethod != 8))
            {
                throw new ArgumentException(String.Format("Unsupported Compression method ({0:X2})", this.CompressionMethod));
            }
        }

        private bool ValidateOutput(String basedir, Stream outstream, out String OutputFile)
        {
            if (basedir != null)
            {
                OutputFile = this.FileName.StartsWith("/") ? Path.Combine(basedir, this.FileName.Substring(1)) : Path.Combine(basedir, this.FileName);
                if (this.IsDirectory || this.FileName.EndsWith("/"))
                {
                    if (!Directory.Exists(OutputFile))
                    {
                        Directory.CreateDirectory(OutputFile);
                    }
                    return true;
                }
                return false;
            }
            if (outstream == null)
            {
                throw new ZipException("Cannot extract.", new ArgumentException("Invalid input.", "outstream | basedir"));
            }
            OutputFile = null;
            return (this.IsDirectory || this.FileName.EndsWith("/"));
        }

        private bool WantReadAgain()
        {
            if (this._CompressionMethod == 0)
            {
                return false;
            }
            if (this._CompressedSize < this._UncompressedSize)
            {
                return false;
            }
            if (this.ForceNoCompression)
            {
                return false;
            }
            return true;
        }

        internal void Write(Stream outstream)
        {
            if (this._Source == EntrySource.Zipfile)
            {
                this.CopyThroughOneEntry(outstream);
            }
            else
            {
                bool readAgain = true;
                int nCycles = 0;
                do
                {
                    nCycles++;
                    this.WriteHeader(outstream, nCycles);
                    if (this.IsDirectory)
                    {
                        break;
                    }
                    this._EmitOne(outstream);
                    if (nCycles > 1)
                    {
                        readAgain = false;
                    }
                    else if (!outstream.CanSeek)
                    {
                        readAgain = false;
                    }
                    else
                    {
                        readAgain = this.WantReadAgain();
                    }
                    if (readAgain)
                    {
                        outstream.Seek((long) this._RelativeOffsetOfHeader, SeekOrigin.Begin);
                        outstream.SetLength(outstream.Position);
                        CountingStream s1 = outstream as CountingStream;
                        if (s1 != null)
                        {
                            s1.Adjust(this._TotalEntrySize);
                        }
                    }
                }
                while (readAgain);
            }
        }

        internal void WriteCentralDirectoryEntry(Stream s)
        {
            byte[] bytes = new byte[0x1000];
            int i = 0;
            bytes[i++] = 80;
            bytes[i++] = 0x4b;
            bytes[i++] = 1;
            bytes[i++] = 2;
            bytes[i++] = this._EntryHeader[4];
            bytes[i++] = this._EntryHeader[5];
            short extraFieldLengthSave = (short) (this._EntryHeader[0x1c] + (this._EntryHeader[0x1d] * 0x100));
            this._EntryHeader[0x1c] = 0;
            this._EntryHeader[0x1d] = 0;
            int j = 0;
            j = 0;
            while (j < 0x1a)
            {
                bytes[i + j] = this._EntryHeader[4 + j];
                j++;
            }
            this._EntryHeader[0x1c] = (byte) (extraFieldLengthSave & 0xff);
            this._EntryHeader[0x1d] = (byte) ((extraFieldLengthSave & 0xff00) >> 8);
            i += j;
            int commentLength = (this._CommentBytes == null) ? 0 : this._CommentBytes.Length;
            if ((commentLength + i) > bytes.Length)
            {
                commentLength = bytes.Length - i;
            }
            bytes[i++] = (byte) (commentLength & 0xff);
            bytes[i++] = (byte) ((commentLength & 0xff00) >> 8);
            bytes[i++] = 0;
            bytes[i++] = 0;
            bytes[i++] = this.IsDirectory ? ((byte) 0) : ((byte) 1);
            bytes[i++] = 0;
            bytes[i++] = this.IsDirectory ? ((byte) 0x10) : ((byte) 0x20);
            bytes[i++] = 0;
            bytes[i++] = 0xb6;
            bytes[i++] = 0x81;
            bytes[i++] = (byte) (this._RelativeOffsetOfHeader & 0xff);
            bytes[i++] = (byte) ((this._RelativeOffsetOfHeader & 0xff00) >> 8);
            bytes[i++] = (byte) ((this._RelativeOffsetOfHeader & 0xff0000) >> 0x10);
            bytes[i++] = (byte) ((this._RelativeOffsetOfHeader & 0xff000000L) >> 0x18);
            short filenameLength = (short) (this._EntryHeader[0x1a] + (this._EntryHeader[0x1b] * 0x100));
            j = 0;
            while (j < filenameLength)
            {
                bytes[i + j] = this._EntryHeader[30 + j];
                j++;
            }
            i += j;
            if (commentLength != 0)
            {
                j = 0;
                while ((j < commentLength) && ((i + j) < bytes.Length))
                {
                    bytes[i + j] = this._CommentBytes[j];
                    j++;
                }
                i += j;
            }
            s.Write(bytes, 0, i);
        }

        private void WriteHeader(Stream s, int cycle)
        {
            byte[] bytes = new byte[0x2200];
            int i = 0;
            bytes[i++] = 80;
            bytes[i++] = 0x4b;
            bytes[i++] = 3;
            bytes[i++] = 4;
            short VersionNeededToExctract = 20;
            bytes[i++] = (byte) (VersionNeededToExctract & 0xff);
            bytes[i++] = (byte) ((VersionNeededToExctract & 0xff00) >> 8);
            byte[] FileNameBytes = this.GetFileNameBytes();
            short filenameLength = (short) FileNameBytes.Length;
            this._CommentBytes = null;
            if (!String.IsNullOrEmpty(this._Comment))
            {
                this._CommentBytes = this._Comment.ToByteArray(this._encoding);
            }
            bool setUtf8Bit = this.UseUtf8Encoding && (ZipSharedUtilities.HighBytes(this._CommentBytes) || ZipSharedUtilities.HighBytes(FileNameBytes));
            this._BitField = ((short) 0);
            if (setUtf8Bit)
            {
                this._BitField = (short) (this._BitField | 0x800);
            }
            if (!s.CanSeek)
            {
                this._BitField = (short) (this._BitField | 8);
            }
            bytes[i++] = (byte) (this._BitField & 0xff);
            bytes[i++] = (byte) ((this._BitField & 0xff00) >> 8);
            if (this.__FileDataPosition == 0L)
            {
                this._UncompressedSize = 0;
                this._CompressedSize = 0;
                this._Crc32 = 0;
            }
            this.FigureCompressionMethodForWriting(cycle);
            bytes[i++] = (byte) (this.CompressionMethod & 0xff);
            bytes[i++] = (byte) ((this.CompressionMethod & 0xff00) >> 8);
            this._TimeBlob = ZipSharedUtilities.DateTimeToPacked(this.LastModified);
            bytes[i++] = (byte) (this._TimeBlob & 0xff);
            bytes[i++] = (byte) ((this._TimeBlob & 0xff00) >> 8);
            bytes[i++] = (byte) ((this._TimeBlob & 0xff0000) >> 0x10);
            bytes[i++] = (byte) ((this._TimeBlob & 0xff000000L) >> 0x18);
            bytes[i++] = (byte) (this._Crc32 & 0xff);
            bytes[i++] = (byte) ((this._Crc32 & 0xff00) >> 8);
            bytes[i++] = (byte) ((this._Crc32 & 0xff0000) >> 0x10);
            bytes[i++] = (byte) ((this._Crc32 & 0xff000000L) >> 0x18);
            bytes[i++] = (byte) (this._CompressedSize & 0xff);
            bytes[i++] = (byte) ((this._CompressedSize & 0xff00) >> 8);
            bytes[i++] = (byte) ((this._CompressedSize & 0xff0000) >> 0x10);
            bytes[i++] = (byte) ((this._CompressedSize & 0xff000000L) >> 0x18);
            bytes[i++] = (byte) (this._UncompressedSize & 0xff);
            bytes[i++] = (byte) ((this._UncompressedSize & 0xff00) >> 8);
            bytes[i++] = (byte) ((this._UncompressedSize & 0xff0000) >> 0x10);
            bytes[i++] = (byte) ((this._UncompressedSize & 0xff000000L) >> 0x18);
            bytes[i++] = (byte) (filenameLength & 0xff);
            bytes[i++] = (byte) ((filenameLength & 0xff00) >> 8);
            byte[] extra = null;
            short ExtraFieldLength = (extra == null) ? ((short) 0) : ((short) extra.Length);
            bytes[i++] = (byte) (ExtraFieldLength & 0xff);
            bytes[i++] = (byte) ((ExtraFieldLength & 0xff00) >> 8);
            int j = 0;
            j = 0;
            while ((j < FileNameBytes.Length) && ((i + j) < bytes.Length))
            {
                bytes[i + j] = FileNameBytes[j];
                j++;
            }
            i += j;
            if (extra != null)
            {
                j = 0;
                while (j < extra.Length)
                {
                    bytes[i + j] = extra[j];
                    j++;
                }
                i += j;
            }
            CountingStream counter = s as CountingStream;
            this._RelativeOffsetOfHeader = (counter != null) ? counter.BytesWritten : ((int) s.Position);
            this._LengthOfHeader = i;
            s.Write(bytes, 0, i);
            this._EntryHeader = new byte[i];
            for (j = 0; j < i; j++)
            {
                this._EntryHeader[j] = bytes[j];
            }
        }

        // Properties
        private Object _streamLock { get { return (Object)_zipfile ?? (Object)_archiveStream; } }
        private Stream ArchiveStream
        {
            get
            {
                if ((this._archiveStream == null) && (this._zipfile != null))
                {
                    this._zipfile.Reset();
                    this._archiveStream = this._zipfile.ReadStream;
                }
                return this._archiveStream;
            }
        }

        public short BitField
        {
            get
            {
                return this._BitField;
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
            }
        }

        public int CompressedSize
        {
            get
            {
                return this._CompressedSize;
            }
        }

        public short CompressionMethod
        {
            get
            {
                return this._CompressionMethod;
            }
            set
            {
                if ((value != 0) && (value != 8))
                {
                    throw new InvalidOperationException("Unsupported compression method. Specify 8 or 0.");
                }
                this._CompressionMethod = value;
                this._ForceNoCompression = this._CompressionMethod == 0;
            }
        }

        public double CompressionRatio
        {
            get
            {
                if (this.UncompressedSize == 0)
                {
                    return 0.0;
                }
                return (100.0 * (1.0 - ((1.0 * this.CompressedSize) / (1.0 * this.UncompressedSize))));
            }
        }

        public int Crc32
        {
            get
            {
                return this._Crc32;
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

        public String FileName
        {
            get
            {
                return this._FileNameInArchive;
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

        public bool IsDirectory
        {
            get
            {
                return this._IsDirectory;
            }
        }

        public DateTime LastModified
        {
            get
            {
                return this._LastModified;
            }
            set
            {
                this._LastModified = value;
            }
        }

        public String LocalFileName
        {
            get
            {
                return this._LocalFileName;
            }
        }

        public bool OverwriteOnExtract
        {
            get
            {
                return this._OverwriteOnExtract;
            }
            set
            {
                this._OverwriteOnExtract = value;
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

        public int UncompressedSize
        {
            get
            {
                return this._UncompressedSize;
            }
        }

        public bool UseUtf8Encoding
        {
            get
            {
                return (this._encoding == Encoding.GetEncoding("UTF-8"));
            }
            set
            {
                this._encoding = value ? Encoding.GetEncoding("UTF-8") : ZipFile.DefaultEncoding;
            }
        }

        public short VersionNeeded
        {
            get
            {
                return this._VersionNeeded;
            }
        }

        public ZipFile ZipFile
        {
            get
            {
                return this._zipfile;
            }
            set
            {
                this._zipfile = value;
            }
        }

        public override String ToString()
        {
            return String.Format("{0} ({1})", FileName, IsDirectory ? "DIR" : "FILE");
        }
    }
}