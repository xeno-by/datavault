using System;
using System.IO;
using System.Text;
using DataVault.Core.Impl.Zip.ZipLib.Exceptions;

namespace DataVault.Core.Impl.Zip.ZipLib
{
    internal class ZipDirEntry
    {
        // Fields
        private String _Comment;
        private int _ExternalFileAttrs;
        private byte[] _Extra;
        private String _FileName;
        private short _InternalFileAttrs;

        // Methods
        private ZipDirEntry()
        {
        }

        internal static bool IsNotValidSig(int signature)
        {
            return (signature != 0x2014b50);
        }

        public static ZipDirEntry Read(Stream s, Encoding expectedEncoding)
        {
            int signature = ZipSharedUtilities.ReadSignature(s);
            if (IsNotValidSig(signature))
            {
                s.Seek(-4L, SeekOrigin.Current);
                if (signature != 0x6054b50L)
                {
                    throw new BadReadException(String.Format("  ZipDirEntry::Read(): Bad signature ({0:X8}) at position 0x{1:X8}", signature, s.Position));
                }
                return null;
            }
            byte[] block = new byte[0x2a];
            if (s.Read(block, 0, block.Length) != block.Length)
            {
                return null;
            }
            int i = 0;
            ZipDirEntry zde = new ZipDirEntry();
            short versionMadeBy = (short) (block[i++] + (block[i++] * 0x100));
            short versionNeeded = (short) (block[i++] + (block[i++] * 0x100));
            short bitField = (short) (block[i++] + (block[i++] * 0x100));
            short compressionMethod = (short) (block[i++] + (block[i++] * 0x100));
            int lastModDateTime = ((block[i++] + (block[i++] * 0x100)) + ((block[i++] * 0x100) * 0x100)) + (((block[i++] * 0x100) * 0x100) * 0x100);
            int crc32 = ((block[i++] + (block[i++] * 0x100)) + ((block[i++] * 0x100) * 0x100)) + (((block[i++] * 0x100) * 0x100) * 0x100);
            int compressedSize = ((block[i++] + (block[i++] * 0x100)) + ((block[i++] * 0x100) * 0x100)) + (((block[i++] * 0x100) * 0x100) * 0x100);
            int uncompressedSize = ((block[i++] + (block[i++] * 0x100)) + ((block[i++] * 0x100) * 0x100)) + (((block[i++] * 0x100) * 0x100) * 0x100);
            short filenameLength = (short) (block[i++] + (block[i++] * 0x100));
            short extraFieldLength = (short) (block[i++] + (block[i++] * 0x100));
            short commentLength = (short) (block[i++] + (block[i++] * 0x100));
            i += 2;
            zde._InternalFileAttrs = (short) (block[i++] + (block[i++] * 0x100));
            zde._ExternalFileAttrs = ((block[i++] + (block[i++] * 0x100)) + ((block[i++] * 0x100) * 0x100)) + (((block[i++] * 0x100) * 0x100) * 0x100);
            block = new byte[filenameLength];
            int n = s.Read(block, 0, block.Length);
            if ((bitField & 0x800) == 0x800)
            {
                zde._FileName = ZipSharedUtilities.Utf8StringFromBuffer(block, block.Length);
            }
            else
            {
                zde._FileName = ZipSharedUtilities.StringFromBuffer(block, block.Length, expectedEncoding);
            }
            if (extraFieldLength > 0)
            {
                zde._Extra = new byte[extraFieldLength];
                n = s.Read(zde._Extra, 0, zde._Extra.Length);
            }
            if (commentLength > 0)
            {
                block = new byte[commentLength];
                n = s.Read(block, 0, block.Length);
                if ((bitField & 0x800) == 0x800)
                {
                    zde._Comment = ZipSharedUtilities.Utf8StringFromBuffer(block, block.Length);
                }
                else
                {
                    zde._Comment = ZipSharedUtilities.StringFromBuffer(block, block.Length, expectedEncoding);
                }
            }
            return zde;
        }

        // Properties
        public String Comment
        {
            get
            {
                return this._Comment;
            }
        }

        public String FileName
        {
            get
            {
                return this._FileName;
            }
        }

        public bool IsDirectory
        {
            get
            {
                return ((this._InternalFileAttrs == 0) && ((this._ExternalFileAttrs & 0x10) == 0x10));
            }
        }
    }
}