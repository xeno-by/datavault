using System.IO;

namespace DataVault.Core.Impl.Zip.ZipLib.Streams
{
    internal class CountingStream : Stream
    {
        // Fields
        private int _bytesRead;
        private int _bytesWritten;
        private Stream _s;

        // Methods
        public CountingStream(Stream s)
        {
            this._s = s;
        }

        public void Adjust(int delta)
        {
            this._bytesWritten -= delta;
        }

        public override void Flush()
        {
            this._s.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int n = this._s.Read(buffer, offset, count);
            this._bytesRead += n;
            return n;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this._s.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this._s.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this._s.Write(buffer, offset, count);
            this._bytesWritten += count;
        }

        // Properties
        public int BytesRead
        {
            get
            {
                return this._bytesRead;
            }
        }

        public int BytesWritten
        {
            get
            {
                return this._bytesWritten;
            }
        }

        public override bool CanRead
        {
            get
            {
                return this._s.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return this._s.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this._s.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return this._s.Length;
            }
        }

        public override long Position
        {
            get
            {
                return this._s.Position;
            }
            set
            {
                this._s.Seek(value, SeekOrigin.Begin);
            }
        }
    }
}