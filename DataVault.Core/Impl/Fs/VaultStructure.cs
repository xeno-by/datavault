using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataVault.Core.Helpers;

namespace DataVault.Core.Impl.Fs
{
    internal static class VaultStructure
    {
        private static readonly String IndexFile = "#index";

        public static void SaveIndexFile(this DirectoryInfo dir, IEnumerable<FileSystemInfo> fses)
        {
            var common = dir.FullName.Slash().Length - 1;
            using (var index = new StreamWriter(dir.FilePathCombine(IndexFile)))
            {
                foreach (var fse in fses)
                {
                    if (fse is FileInfo)
                    {
                        if (File.Exists(fse.FullName))
                        {
                            index.WriteLine(fse.FullName.Unslash().Substring(common));
                        }
                        else if (File.Exists(fse.FullName.Unslash().DoubleBux()))
                        {
                            index.WriteLine(fse.FullName.Unslash().DoubleBux().Substring(common));
                        }
                        else
                        {
                            throw new ArgumentException("Something was overlooked");
                        }
                    }
                    else if (fse is DirectoryInfo)
                    {
                        if (Directory.Exists(fse.FullName.Slash()))
                        {
                            index.WriteLine(fse.FullName.Slash().Substring(common));
                        }
                        else if (Directory.Exists(fse.FullName.Unslash().DoubleBux().Slash()))
                        {
                            index.WriteLine(fse.FullName.Unslash().DoubleBux().Slash().Substring(common));
                        }
                        else
                        {
                            throw new ArgumentException("Something was overlooked");
                        }
                    }
                    else
                    {
                        throw new NotSupportedException(fse == null ? null : fse.ToString());
                    }
                }
            }
        }

        public static IEnumerable<FileSystemInfo> LoadIndexFile(this DirectoryInfo dir)
        {
            var index = dir.FullName.FilePathCombine(IndexFile);
            return !File.Exists(index) ? null : File.ReadAllText(index)
                .Split(new String[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .SelectMany(s => LoadRelatedFsObjects(dir.FullName.Unslash() + s))
                .Concat(LoadRelatedFsObjects(dir.FullName.Slash()));
        }

        private static IEnumerable<FileSystemInfo> LoadRelatedFsObjects(String s)
        {
            var fsi = s.AsFileSystemInfo();
            yield return fsi;

            if (fsi is FileInfo)
            {
                var fsiUnbuxt = fsi.FullName.Reverse().SkipWhile(c => c == '$').Reverse().StringJoin(String.Empty);
                var satellite = fsiUnbuxt + "$";
                if (File.Exists(satellite)) yield return new FileInfo(satellite);
            }
            else if (fsi is DirectoryInfo)
            {
                var satellite = fsi.FullName.Slash() + "$";
                if (File.Exists(satellite)) yield return new FileInfo(satellite);
            }
            else
            {
                throw new NotSupportedException(fsi == null ? "null" : fsi.GetType().ToString());
            }
        }

        public static IEnumerable<FsVaultEntry> BuildVaultEntries(this String path)
        {
            return new DirectoryInfo(path).BuildVaultEntries();
        }

        public static IEnumerable<FsVaultEntry> BuildVaultEntries(this DirectoryInfo dir)
        {
            if (!dir.Exists) return Enumerable.Empty<FsVaultEntry>();
            var files = dir.LoadIndexFile() ?? dir.Flatten(d => d.GetDirectories()).SelectMany(di => di.GetFileSystemInfos());
            return files.Select(fsi => new FsVaultEntry(dir, fsi)).ToArray();
        }

        public static IEnumerable<FsVaultEntry> BuildVaultEntries(this VaultBase vault, String uri)
        {
            var rootMetaFso = uri.FilePathCombine("$").AsFileSystemInfo();
            if (rootMetaFso != null && rootMetaFso.Exists)
            {
                yield return new FsVaultEntry(uri, rootMetaFso);
            }

            foreach (var b in vault.Root.GetBranchesRecursive())
            {
                var dirname = uri.DirPathCombine(b.VPath);

                var fso = dirname.MapPathToCoexisting();
                if (fso == null || !fso.Exists)
                {
                    throw new ArgumentException(String.Format(
                        "There's no FS Object (even at a coexisting path) that corresponds to a branch '{0}'", b));
                }

                var metaFso = fso.FullName.Slash().FilePathCombine("$").AsFileSystemInfo();
                if (metaFso != null && metaFso.Exists)
                {
                    yield return new FsVaultEntry(uri, metaFso);
                }

                yield return new FsVaultEntry(uri, fso);
            }

            foreach (var v in vault.Root.GetValuesRecursive(ValueKind.RegularAndInternal))
            {
                var fileName = uri.FilePathCombine(v.VPath);

                var fso = fileName.MapPathToCoexisting();
                if (fso == null || !fso.Exists)
                {
                    throw new ArgumentException(String.Format(
                        "There's no FS Object (even at a coexisting path) that corresponds to a value '{0}'", v));
                }

                var fileUnbuxt = fso.FullName.Unslash().Reverse().SkipWhile(c => c == '$').Reverse().StringJoin(String.Empty);
                var metaFso = (fileUnbuxt + "$").AsFileSystemInfo();
                if (metaFso != null && metaFso.Exists)
                {
                    yield return new FsVaultEntry(uri, metaFso);
                }

                yield return new FsVaultEntry(uri, fso);
            }
        }
    }
}