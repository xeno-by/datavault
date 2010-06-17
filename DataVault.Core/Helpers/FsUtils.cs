using System;
using System.IO;

namespace DataVault.Core.Helpers
{
    public static class FsUtils
    {
        public static String FilePathCombine(this DirectoryInfo p1, String p2)
        {
            return p1.FullName.FilePathCombine(p2);
        }

        public static String DirPathCombine(this DirectoryInfo p1, String p2)
        {
            return p1.FullName.DirPathCombine(p2);
        }

        public static String FilePathCombine(this String p1, String p2)
        {
            if (!p1.EndsWith(@"\")) p1 += @"\";
            if (p2.StartsWith(@"\")) p2 = p2.Substring(1);
            return p1 + p2;
        }

        public static String DirPathCombine(this String p1, String p2)
        {
            if (!p1.EndsWith(@"\")) p1 += @"\";
            if (p2.StartsWith(@"\")) p2 = p2.Substring(1);

            var res = p1 + p2;
            if (!res.EndsWith(@"\")) res += @"\";
            return res;
        }

        public static FileSystemInfo AsFileSystemInfo(this String path)
        {
            return path.EndsWith(@"\") ? new DirectoryInfo(path) : (FileSystemInfo)new FileInfo(path);
        }

        public static DirectoryInfo Parent(this FileSystemInfo fsi)
        {
            if (fsi is FileInfo)
            {
                return ((FileInfo)fsi).Directory;
            }
            else if (fsi is DirectoryInfo)
            {
                return ((DirectoryInfo)fsi).Parent;
            }
            else
            {
                throw new NotSupportedException(fsi == null ? "null" : fsi.ToString());
            }
        }

        public static void DeleteRecursive(this FileSystemInfo fsi)
        {
            if (fsi is FileInfo)
            {
                fsi.Delete();
            }
            else if (fsi is DirectoryInfo)
            {
                ((DirectoryInfo)fsi).Delete(true);
            }
            else
            {
                throw new NotSupportedException(fsi == null ? "null" : fsi.ToString());
            }
        }

        public static void MoveContentTo(this String source, String target)
        {
            MoveContentTo(new DirectoryInfo(source), new DirectoryInfo(target));
        }

        public static void MoveContentTo(this String source, DirectoryInfo target)
        {
            MoveContentTo(new DirectoryInfo(source), target);
        }

        public static void MoveContentTo(this DirectoryInfo source, String target)
        {
            MoveContentTo(source, new DirectoryInfo(target));
        }

        public static void MoveContentTo(this DirectoryInfo source, DirectoryInfo target)
        {
            foreach(var fileFromRoot in source.GetFiles())
                fileFromRoot.MoveTo(target.FilePathCombine(fileFromRoot.Name));

            foreach (var dirFromRoot in source.GetDirectories())
                dirFromRoot.MoveTo(target.FilePathCombine(dirFromRoot.Name));
        }
    }
}