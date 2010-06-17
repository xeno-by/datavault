using System;
using System.IO;

namespace DataVault.Core.Impl.Zip.ZipLib.Streams
{
    internal class CrcCalculatorStream : Stream
    {
        // Fields
        private CRC32 _Crc32;
        private Stream _InnerStream;
        private int _length;

        // Methods
        public CrcCalculatorStream(Stream stream)
        {
            this._length = 0;
            this._InnerStream = stream;
            this._Crc32 = new CRC32();
        }

        public CrcCalculatorStream(Stream stream, int length)
        {
            this._length = 0;
            this._InnerStream = stream;
            this._Crc32 = new CRC32();
            this._length = length;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesToRead = count;
            if (this._length != 0)
            {
                if (this._Crc32.TotalBytesRead >= this._length)
                {
                    return 0;
                }
                int bytesRemaining = this._length - this._Crc32.TotalBytesRead;
                if (bytesRemaining < count)
                {
                    bytesToRead = bytesRemaining;
                }
            }
            int n = this._InnerStream.Read(buffer, offset, bytesToRead);
            if (n > 0)
            {
                this._Crc32.SlurpBlock(buffer, offset, n);
            }
            return n;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count > 0)
            {
                this._Crc32.SlurpBlock(buffer, offset, count);
            }
            this._InnerStream.Write(buffer, offset, count);
        }

        // Properties
        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public int Crc32
        {
            get
            {
                return this._Crc32.Crc32Result;
            }
        }

        public override long Length
        {
            get
            {
                if (this._length == 0)
                {
                    throw new NotImplementedException();
                }
                return (long) this._length;
            }
        }

        public override long Position
        {
            get
            {
                return (long) this._Crc32.TotalBytesRead;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int TotalBytesSlurped
        {
            get
            {
                return this._Crc32.TotalBytesRead;
            }
        }
    }
}