using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using DataVault.Core.Helpers;

namespace DataVault.Core.Impl.Fs
{
    internal static class FileDirCoexistence
    {
        private static IEnumerable<String> PathParentHierarchy(String fspath)
        {
            var initialFsi = fspath.AsFileSystemInfo();
            for (var curr = initialFsi.Parent(); curr != null; curr = curr.Parent())
                yield return curr.FullName.Slash();
        }

        private static IEnumerable<String> PathParentSteps(String path)
        {
            var parents = PathParentHierarchy(path).Reverse().ToArray();

            var prev = String.Empty;
            foreach(var p in parents)
            {
                yield return p.Substring(prev.Length);
                prev = p;
            }
        }

        public static String GetAvailDirCPath(this String dirPath)
        {
            dirPath = dirPath.Slash();

            // prevents incorrect treatment of paths with elements ending with a dot
            dirPath = dirPath.Replace(".\\", ".$$\\");
            if (dirPath.EndsWith(".")) dirPath += "$$";

            var sb = new StringBuilder();
            foreach(var step in PathParentSteps(dirPath)
                .Concat(dirPath.AsFileSystemInfo().Name.MkArray()))
            {
                if (File.Exists(sb + step.Unslash()))
                {
                    sb.Append(step.Unslash().DoubleBux().Slash());
                }
                else
                {
                    sb.Append(step);
                }
            }

            return sb.ToString().Slash();
        }

        public static String GetAvailFileCPath(this String filePath)
        {
            filePath = filePath.Unslash();

            // prevents incorrect treatment of paths with elements ending with a dot
            filePath = filePath.Replace(".\\", ".$$\\");
            if (filePath.EndsWith(".")) filePath += "$$";

            var parentDir = Path.GetDirectoryName(filePath);
            var localName = Path.GetFileName(filePath);

            var fname = GetAvailDirCPath(parentDir) + localName;
            return Directory.Exists(fname) ? fname.DoubleBux() : fname;
        }

        public static FileSystemInfo MapPathToCoexisting(this String fspath)
        {
            // prevents incorrect treatment of paths with elements ending with a dot
            fspath = fspath.Replace(".\\", ".$$\\");
            if (fspath.EndsWith(".")) fspath += "$$";

            var sb = new StringBuilder();
            foreach (var step in PathParentSteps(fspath))
            {
                if (!step.EndsWith(".") && Directory.Exists(sb + step))
                {
                    sb.Append(step);
                }
                else if (Directory.Exists(sb + step.Unslash().DoubleBux().Slash()))
                {
                    sb.Append(step.Unslash().DoubleBux().Slash());
                }
                else if (Directory.Exists(sb + step.Unslash().DoubleBux().DoubleBux().Slash()))
                {
                    sb.Append(step.Unslash().DoubleBux().DoubleBux().Slash());
                }
                else
                {
                    return null;
                }
            }

            var parent = sb.ToString();
            var localPath = fspath.AsFileSystemInfo().Name;

            if (fspath.EndsWith(@"\"))
            {
                if (!fspath.EndsWith(".") && Directory.Exists(parent + localPath))
                {
                    return new DirectoryInfo(parent + localPath);
                }
                else if (Directory.Exists(parent + localPath.Unslash().DoubleBux().Slash()))
                {
                    return new DirectoryInfo(parent + localPath.Unslash().DoubleBux().Slash());
                }
                else if (Directory.Exists(parent + localPath.Unslash().DoubleBux().DoubleBux().Slash()))
                {
                    return new DirectoryInfo(parent + localPath.Unslash().DoubleBux().DoubleBux().Slash());
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (!localPath.EndsWith(".") && File.Exists(parent + localPath))
                {
                    return new FileInfo(parent + localPath);
                }
                else if (File.Exists(parent + localPath.Unslash().DoubleBux()))
                {
                    return new FileInfo(parent + localPath.Unslash().DoubleBux());
                }
                else if (File.Exists(parent + localPath.Unslash().DoubleBux().DoubleBux()))
                {
                    return new FileInfo(parent + localPath.Unslash().DoubleBux().DoubleBux());
                }
                else
                {
                    return null;
                }
            }
        }
    }
}