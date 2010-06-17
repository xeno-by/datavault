using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DataVault.Core.Api;
using DataVault.Core.Helpers.Assertions;
using DataVault.Core.Impl.Api;
using DataVault.Core.Helpers;
using DataVault.Core.Impl.Zip.ZipLib;
using System.Linq;

namespace DataVault.Core.Impl.Zip
{
    internal class ZipVault : VaultBase
    {
        private ZipFile Zip { get; set; }
        public override String Uri { get { return Path.GetFullPath(Zip.Name).Unslash(); } }

        public ZipVault(String fileName) 
        {
            using (InternalExpose())
            {
                Zip = new ZipFile(fileName, Encoding.UTF8);
                var zeIndex = Zip.Entries.ToDictionary(ze => ze.FileName, ze => ze);
                Root = new Branch(this, null, null);

                foreach (var value in Zip.Entries.Where(e => !e.IsDirectory && !e.FileName.EndsWith("$")))
                {
                    var contentFile = value.FileName;
                    var metadataFile = contentFile + "$";

                    var valueNode = (Value)Root.CreateValue(value.FileName, ValueKind.RegularAndInternal);
                    valueNode.SetContent(() => zeIndex[contentFile].ExtractEager());
                    valueNode.RawSetMetadata(() => zeIndex.GetOrDefault(metadataFile).ExtractEager());
                }

                foreach (var branch in Zip.Entries.Where(e => e.IsDirectory && !e.FileName.EndsWith("$")))
                {
                    // no need for slash, because dirname includes it
                    var metadataFile = branch.FileName + "$";

                    // some of those branches will already be created by this time
                    var branchNode = (Branch)Root.GetOrCreateBranch(branch.FileName);
                    branchNode.RawSetMetadata(() => zeIndex.GetOrDefault(metadataFile).ExtractEager());
                }

                // for backward compatibility (DO NOT REMOVE again, please)
                var oldVersionStorage = Zip.Entries.SingleOrDefault(e => !e.IsDirectory && 
                    e.FileName == "version$" || e.FileName == "/version$" || e.FileName == "\\version$");
                if (oldVersionStorage != null)
                {
                    var matchingValue = Root.GetValue("version");
                    var possibleOverlap = Root.GetValue("$version");
                    if (matchingValue == null && possibleOverlap == null)
                    {
                        var versionHolder = Root.CreateValue("$version", ValueKind.Regular);
                        versionHolder.SetContent(() => zeIndex[oldVersionStorage.FileName].ExtractEager());
                    }
                }

                // root metadata requires special treatment since root doesn't get enumerated
                Root.RawSetMetadata(() => (zeIndex.GetOrDefault("/$") ?? zeIndex.GetOrDefault("$")).ExtractEager());

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
                    SaveImpl(Uri, true);
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

                    SaveImpl(uri, true);
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
                    var bakUri = Uri + ".bak";
                    if (File.Exists(bakUri)) File.Delete(bakUri);
                    SaveImpl(bakUri, false);
                    return this;
                }
            }
        }

        private void SaveImpl(String fileName, bool rebuildZipEntries)
        {
            using (ExposeReadOnly())
            {
                var newzip = new ZipFile(){Encoding = Encoding.UTF8};

                // bug. here we face a potential tho very unprobable sync problem
                // if we've imported some nodes from another vault and are now unbinding them
                // it's possible that the vault will right now undergo certain changes that
                // won't be propagated to the nodes we've just unbound
                Root.GetValuesRecursive(ValueKind.RegularAndInternal).ForEach(Bind);
                Root.GetBranchesRecursive().ForEach(Bind);

                // mapping between values/branches and entries in the new file
                var newZeIndex = new Dictionary<IElement, String>();

                // save all values -> this will also automatically create corresponding branches
                foreach (Value value in Root.GetValuesRecursive(ValueKind.RegularAndInternal))
                {
                    var contentStream = value.ContentStream.FixupForBeingSaved();
                    var valueZe = newzip.AddFileStream(value.Name, value.VPath.Parent.ToZipPathDir(), contentStream);
                    newZeIndex.Add(value, valueZe.FileName);

                    if (value.Metadata.Raw != null)
                        newzip.AddFileStream(value.Name + "$", value.VPath.Parent.ToZipPathDir(), value.Metadata.Raw.AsStream());
                }

                // despite of the previous step having created the branches, 
                // we still need to explicitly add them in order to store the metadata
                foreach(Branch branch in Root.GetBranchesRecursive())
                {
                    var branchZe = newzip.AddDirectoryByName(branch.VPath.ToZipPathDir());
                    newZeIndex.Add(branch, branchZe.FileName);

                    if (branch.Metadata.Raw != null)
                        newzip.AddFileStream("$", branch.VPath.ToZipPathDir(), branch.Metadata.Raw.AsStream());
                }

                // root metadata requires special treatment since root doesn't get enumerated
                if (Root.Metadata.Raw.IsNeitherNullNorEmpty())
                    newzip.AddFileStream("$", String.Empty.ToZipPathDir(), Root.Metadata.Raw.AsStream());

                if (rebuildZipEntries)
                {
                    GC.Collect(); // is this really necessary here?
                    var deletedButStillAlive = BoundElements.Select(wr => wr.IsAlive ? (IElement)wr.Target : null)
                        .Where(el => el != null)
                        .Except(Root.GetValuesRecursive(ValueKind.RegularAndInternal).Cast<IElement>())
                        .Except(Root.GetBranchesRecursive().Cast<IElement>())
                        .Except(Root.MkArray())
                        .Distinct();

                    // fixup metadata/content streams of deleted and not yet gcollected nodes
                    Action<Action> neverFail = a => { try { a(); } catch { /* just ignore */ } };
                    deletedButStillAlive.ForEach(el => neverFail(() => el.CacheInMemory()));

                    // only now can we dispose the previous zip instance
                    // previously it was necessary to extract streams we're going to repack
                    if (Zip != null)
                    {
                        Zip.Dispose();
                    }

                    Zip = newzip;
                    newzip.Save(fileName);

                    // fixup content/metadata streams to reference the new file/vpaths
                    var opt = Zip.Entries.ToDictionary(ze => ze.FileName, ze => ze);
                    foreach (Value value in Root.GetValuesRecursive(ValueKind.RegularAndInternal))
                    {
                        var contentFile = newZeIndex[value];
                        var metadataFile = contentFile + "$";

                        value.SetContent(() => opt[contentFile].ExtractEager());
                        value.RawSetMetadata(() => opt.GetOrDefault(metadataFile).ExtractEager());
                    }

                    foreach (Branch branch in Root.GetBranchesRecursive())
                    {
                        var metadataFile = newZeIndex[branch] + "$";
                        branch.RawSetMetadata(() => opt.GetOrDefault(metadataFile).ExtractEager());
                    }

                    // root metadata requires special treatment since root doesn't get enumerated
                    Root.RawSetMetadata(() => (opt.GetOrDefault("/$") ?? opt.GetOrDefault("$")).ExtractEager());

                    // set the changes in stone
                    Root.AfterSave();
                    Root.GetBranchesRecursive().Cast<Branch>().ForEach(b => b.AfterSave());
                    Root.GetValuesRecursive(ValueKind.RegularAndInternal).Cast<Value>().ForEach(v => v.AfterSave());
                }
                else
                {
                    if (Uri == fileName)
                    {
                        throw new InvalidOperationException("Saving vault into its source file requires rebuilding ZIP entries.");
                    }
                    else
                    {
                        newzip.Save(fileName);
                        newzip.Dispose();
                    }
                }
            }
        }

        public override void Dispose()
        {
            using (InternalExpose())
            {
                using (ExposeReadWrite())
                {
                    if (Zip != null) Zip.Dispose();
                    base.Dispose();
                }
            }
        }
    }
}