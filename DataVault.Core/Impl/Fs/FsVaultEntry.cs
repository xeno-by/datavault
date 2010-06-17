using System;
using System.Collections.Generic;
using System.IO;
using DataVault.Core.Api;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;

namespace DataVault.Core.Impl.Fs
{
    internal class FsVaultEntry : IDisposable
    {
        public DirectoryInfo Root { get; private set; }
        public FileSystemInfo FsItem { get; private set; }
        internal List<FileStream> Streams { get; private set; }

        private String _absPathCached;
        private String _relPathCached;
        private VPath _vpathCached;

        public FsVaultEntry(String root, FileSystemInfo fsItem)
            : this(new DirectoryInfo(root), fsItem)
        {
        }

        public FsVaultEntry(DirectoryInfo root, FileSystemInfo fsItem)
        {
            Root = root;

            FsItem = fsItem;
            FsItem.Exists.AssertTrue();
            FsItem.FullName.StartsWith(Root.FullName).AssertTrue();
            Streams = new List<FileStream>();

            _absPathCached = FsItem.FullName;
            _relPathCached = _absPathCached.Substring(Root.FullName.Length);
            if (FsItem is DirectoryInfo && !_relPathCached.EndsWith(@"\")) _relPathCached += @"\";
            if (!_relPathCached.StartsWith(@"\")) _relPathCached = @"\" + _relPathCached;
            try { _vpathCached = new VPath(_relPathCached.Unbux()); } catch { }
        }

        public String AbsPath { get { return _absPathCached; } }
        public String RelPath { get { return _relPathCached; } }
        public VPath VPath { get { return _vpathCached; } }

        public bool IsBranch { get { return FsItem is DirectoryInfo; } }
        public bool IsValue { get { return !IsBranch && !IsSatellite; } }
        public bool IsSatellite { get { return AbsPath.Unbux().EndsWith("$"); } }

        public Stream ExtractEager()
        {
            lock (this)
            {
                if (!_disposed)
                {
                    var fi = (FsItem as FileInfo).AssertNotNull();
                    var fs = fi.OpenRead();
                    Streams.Add(fs);
                    return fs;
                }
                else
                {
                    throw new ObjectDisposedException(this.ToString());
                }
            }
        }

        public override String ToString()
        {
            return String.Format("{0} (abs: {1})", RelPath, AbsPath);
        }

        private bool _disposed;
        public void Dispose()
        {
            lock (this)
            {
                if (!_disposed)
                {
                    Streams.ForEach(s => s.Dispose());
                    _disposed = true;
                }
            }
        }
    }
}