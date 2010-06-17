using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataVault.Core.Api;
using DataVault.Core.Helpers.Assertions;
using DataVault.Core.Impl.Api;
using DataVault.Core.Helpers;

namespace DataVault.Core.Impl.Fs
{
    internal class FsVault : VaultBase
    {
        public override String Uri { get { return Dir.FullName.Slash(); } }

        private DirectoryInfo _dir;
        private DirectoryInfo Dir
        {
            get { return _dir; }
            set
            {
                _dir = value;
                Entries = Dir.BuildVaultEntries().ToArray();
            }
        }

        private FsVaultEntry[] _entries;
        private FsVaultEntry[] Entries
        {
            get { return _entries; }
            set
            {
                DisposeEntries();
                _entries = value;
            }
        }

        public FsVault(String dirName)
        {
            using (InternalExpose())
            {
                Dir = new DirectoryInfo(dirName);
                var index = Entries.ToDictionary(e => e.RelPath, e => e);
                Root = new Branch(this, null, null);

                foreach (var value in Entries.Where(e => e.IsValue))
                {
                    var contentFile = value;

                    var cfileUnbuxt = value.RelPath.Reverse().SkipWhile(c => c == '$').Reverse().StringJoin(String.Empty);
                    var metadataFile = index.GetOrDefault(cfileUnbuxt + "$");

                    var valueNode = (Value)Root.CreateValue(value.VPath, ValueKind.RegularAndInternal);
                    valueNode.SetContent(contentFile.ExtractEager);
                    if (metadataFile != null) valueNode.RawSetMetadata(metadataFile.ExtractEager);
                }

                foreach (var branch in Entries.Where(e => e.IsBranch))
                {
                    // no need for slash since relpath already contains it
                    var metadataFile = index.GetOrDefault(branch.RelPath + "$");

                    // this permits duplicate directory entries
                    // it's ugly, but i'm cba to fix this at the moment
                    var branchNode = (Branch)Root.GetOrCreateBranch(branch.VPath);
                    if (metadataFile != null) branchNode.RawSetMetadata(metadataFile.ExtractEager);
                }

                // for backward compatibility (DO NOT REMOVE again, please)
                var oldVersionStorage = Entries.SingleOrDefault(e => e.IsValue && e.VPath == "version$");
                if (oldVersionStorage != null)
                {
                    var matchingValue = Root.GetValue("version");
                    var possibleOverlap = Root.GetValue("$version");
                    if (matchingValue == null && possibleOverlap == null)
                    {
                        var versionHolder = Root.CreateValue("$version", ValueKind.Regular);
                        versionHolder.SetContent(oldVersionStorage.ExtractEager);
                    }
                }

                Root.AfterLoad();
                Root.GetBranchesRecursive().Cast<Branch>().ForEach(b => b.AfterLoad());
                Root.GetValuesRecursive(ValueKind.RegularAndInternal).Cast<Value>().ForEach(v => v.AfterLoad());

                (Id != Guid.Empty).AssertTrue();
                (Revision is UInt64).AssertTrue();
            }
        }

        public override IVault Save()
        {
            using (InternalExpose())
            {
                using (ExposeReadWrite())
                {
                    PreSaveFixup();
                    SmartSave(Uri, true);
                    PostSaveFixup();

                    return this;
                }
            }
        }

        public override IVault SaveAs(String uri)
        {
            using (InternalExpose())
            {
                using (ExposeReadWrite())
                {
                    Id = Guid.NewGuid();
                    Revision = 0;

                    PreSaveFixup();
                    SmartSave(uri, true);
                    PostSaveFixup();

                    return this;
                }
            }
        }

        public override IVault Backup()
        {
            using (InternalExpose())
            {
                using (ExposeReadWrite())
                {
                    var info = new DirectoryInfo(Uri);
                    var bakUri = info.Parent == null ? info.FullName.Slash() + "#bak"
                        : info.Parent.FullName.Slash() + info.Name + ".bak";

                    SmartSave(bakUri, false);
                    var bakEntries = this.BuildVaultEntries(bakUri).ToArray();
                    new DirectoryInfo(bakUri).SaveIndexFile(bakEntries.Select(fse => fse.FsItem));

                    return this;
                }
            }
        }

        private void SmartSave(String uri, bool rebuildEntries)
        {
            Func<VPath, String> abs = v => uri.DirPathCombine(v);
            Action<String, Stream> wat = (f, s) => File.WriteAllBytes(f, s.AsByteArray());
            Action<String> del = File.Delete;

            // here we need to defer actual save before everything is closed
            // however we can't close related streams right now since there
            // might exist pending save operations that rely on those streams
            var deferred = new List<Action>();

            foreach (var b in Root.MkArray().Concat(Root.GetBranchesRecursive()).Cast<Branch>())
            {
                var dirname = abs(b.VPath).GetAvailDirCPath();
                if (!Directory.Exists(dirname)) Directory.CreateDirectory(dirname);

                var satellite = abs(b.VPath + "$").GetAvailFileCPath();
                if (b.Metadata.Raw != null)
                {
                    if (!File.Exists(satellite) || b.SaveMyMetadataPlease)
                    {
                        var uptodateMetadata = b.Metadata.Raw.AsStream();
                        deferred.Add(() => wat(satellite, uptodateMetadata));

                        if (rebuildEntries)
                        {
                            // warning: this invalidates stream links, so we need to fixup them later
                            b.RawSetMetadata(() => this.Entries.Where(e => e.IsSatellite)
                                .Single(e => e.AbsPath == satellite).ExtractEager());
                        }
                    }
                }
                else
                {
                    if (File.Exists(satellite))
                    {
                        deferred.Add(() => del(satellite));
                    }
                }
            }

            // save only stuff that needs it
            foreach (var v in Root.GetValuesRecursive(ValueKind.RegularAndInternal).Cast<Value>())
            {
                // it's ok if from the first time you won't understand what I'm doing here
                // but believe me I had no better option...
                var fwd_saveMetadata = v.SaveMyMetadataPlease;
                var fwd_metadataRaw = v.Metadata.Raw;

                var main = abs(v.VPath).GetAvailFileCPath();
                if (!File.Exists(main) || v.SaveMyContentPlease)
                {
                    var uptodateContent = v.ContentStream.CacheInMemory().FixupForBeingSaved();
                    deferred.Add(() => wat(main, uptodateContent));

                    // we must save the info about metadata precisely here:
                    // after the content has been acquired, and before it has been overwritten
                    // Jesus, what a mess of side-effects this library has become...
                    fwd_saveMetadata = v.SaveMyMetadataPlease;
                    fwd_metadataRaw = v.Metadata.Raw;

                    if (rebuildEntries)
                    {
                        // warning: this invalidates stream links, so we need to fixup them later
                        v.SetContent(() => this.Entries.Where(e => e.IsValue)
                           .Single(e => e.AbsPath == main).ExtractEager());
                    }
                }

                // never below we must use the metadata value, since it has been distorted by SetContent
                var satellite = abs(v.VPath.ToString().Unslash() + "$").GetAvailFileCPath();
                if (fwd_metadataRaw != null)
                {
                    if (!File.Exists(satellite) || fwd_saveMetadata)
                    {
                        var uptodateMetadata = fwd_metadataRaw.AsStream();
                        deferred.Add(() => wat(satellite, uptodateMetadata));

                        if (rebuildEntries)
                        {
                            // warning: this invalidates stream links, so we need to fixup them later
                            v.RawSetMetadata(() => this.Entries.Where(e => e.IsSatellite)
                                .Single(e => e.AbsPath == satellite).ExtractEager());
                        }
                    }
                }
                else
                {
                    if (File.Exists(satellite))
                    {
                        deferred.Add(() => del(satellite));
                    }
                }
            }

            // todo. delete files and folders we don't need anymore
            // load current vault index to determine that stuff
            // NB: be careful not to kill everything around (i.e. some vault-unrelated content)

            // warning: this invalidates all currently open streams
            // warning2: this also invalidates stream links, so we need to fixup them later
            // actually they're already fixed just before the moment of dispose
            if (rebuildEntries)
            {
                DisposeEntries();
            }

            // and only now we can save all the deferred stuff with 100% reliability
            // without fear to have one of files to rewrite being locked by a reader
            deferred.ForEach(pendingSaveOrDelete => pendingSaveOrDelete());

            if (rebuildEntries)
            {
                // rebuild the vault structure index
                Entries = this.BuildVaultEntries(uri).ToArray();
                _dir = new DirectoryInfo(uri);
                Dir.SaveIndexFile(Entries.Where(fse => !fse.IsSatellite).Select(fse => fse.FsItem));
            }
        }

        private void PreSaveFixup()
        {
            // claim the responsibility to fixup all tree elements
            Root.GetValuesRecursive(ValueKind.RegularAndInternal).ForEach(Bind);
            Root.GetBranchesRecursive().ForEach(Bind);

            GC.Collect(); // is this really necessary here?
            var deletedButStillAlive = BoundElements.Select(wr => wr.IsAlive ? (IElement)wr.Target : null)
                .Where(el => el != null)
                .Except(Root.GetValuesRecursive(ValueKind.RegularAndInternal).Cast<IElement>())
                .Except(Root.GetBranchesRecursive().Cast<IElement>())
                .Except(Root.MkArray())
                .Distinct();

            // fixup content streams of deleted and not yet gcollected nodes
            Action<Action> neverFail = a => { try { a(); } catch { /* just ignore */ } };
            deletedButStillAlive
                 // screw this - it's too slow to cache a gazillion of small files
//                .ForEach(el => el.CacheInMemory())
                .ForEach(el =>
                {
                    if (el is Value)
                    {
                        var v = (Value)el;
                        neverFail(() => v.SetContent(() => { throw new InvalidOperationException(
                            "FS vault doesn't guarantee consistency of nodes not directly in the tree.");}));}

                    if (el is Element)
                    {
                        var an = (Element)el;
                        neverFail(() => an.RawSetMetadata(() => {throw new InvalidOperationException(
                            "FS vault doesn't guarantee consistency of nodes not directly in the tree.");}));}
                });
        }

        private void PostSaveFixup()
        {
            // set the changes in stone
            Root.AfterSave();
            Root.GetBranchesRecursive().Cast<Branch>().ForEach(b => b.AfterSave());
            Root.GetValuesRecursive(ValueKind.RegularAndInternal).Cast<Value>().ForEach(v => v.AfterSave());
        }

        private void DisposeEntries()
        {
            using (ExposeReadOnly())
            {
                (Entries ?? new FsVaultEntry[0]).ForEach(e => e.Dispose());
            }
        }

        public override void Dispose()
        {
            using (InternalExpose())
            {
                using (ExposeReadWrite())
                {
                    DisposeEntries();
                    base.Dispose();
                }
            }
        }
    }
}