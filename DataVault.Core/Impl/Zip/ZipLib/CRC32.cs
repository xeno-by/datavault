using System;
using System.IO;
using DataVault.Core.Impl.Zip.ZipLib.Exceptions;

namespace DataVault.Core.Impl.Zip.ZipLib
{
    internal class CRC32
    {
        // Fields
        private uint _RunningCrc32Result = uint.MaxValue;
        private int _TotalBytesRead;
        private const int BUFFER_SIZE = 0x2000;
        private static uint[] crc32Table;

        // Methods
        static CRC32()
        {
            uint dwPolynomial = 0xedb88320;
            crc32Table = new uint[0x100];
            for (uint i = 0; i < 0x100; i++)
            {
                uint dwCrc = i;
                for (uint j = 8; j > 0; j--)
                {
                    if ((dwCrc & 1) == 1)
                    {
                        dwCrc = (dwCrc >> 1) ^ dwPolynomial;
                    }
                    else
                    {
                        dwCrc = dwCrc >> 1;
                    }
                }
                crc32Table[i] = dwCrc;
            }
        }

        public int ComputeCrc32(int W, byte B)
        {
            return this.ComputeCrc32((uint) W, B);
        }

        internal int ComputeCrc32(uint W, byte B)
        {
            return (int) (crc32Table[(int) ((IntPtr) ((W ^ B) & 0xff))] ^ (W >> 8));
        }

        public int GetCrc32(Stream input)
        {
            return this.GetCrc32AndCopy(input, null);
        }

        public int GetCrc32AndCopy(Stream input, Stream output)
        {
            if (input == null)
            {
                throw new ZipException("bad input.", new ArgumentException("The input stream must not be null.", "input"));
            }
            byte[] buffer = new byte[0x2000];
            int readSize = 0x2000;
            this._TotalBytesRead = 0;
            int count = input.Read(buffer, 0, readSize);
            if (output != null)
            {
                output.Write(buffer, 0, count);
            }
            this._TotalBytesRead += count;
            while (count > 0)
            {
                this.SlurpBlock(buffer, 0, count);
                count = input.Read(buffer, 0, readSize);
                if (output != null)
                {
                    output.Write(buffer, 0, count);
                }
                this._TotalBytesRead += count;
            }
            return (int) ~this._RunningCrc32Result;
        }

        public void SlurpBlock(byte[] block, int offset, int count)
        {
            if (block == null)
            {
                throw new ZipException("Bad buffer.", new ArgumentException("The data buffer must not be null.", "block"));
            }
            for (int i = 0; i < count; i++)
            {
                int x = offset + i;
                this._RunningCrc32Result = (this._RunningCrc32Result >> 8) ^ crc32Table[(int) ((IntPtr) (block[x] ^ (this._RunningCrc32Result & 0xff)))];
            }
            this._TotalBytesRead += count;
        }

        // Properties
        public int Crc32Result
        {
            get
            {
                return (int) ~this._RunningCrc32Result;
            }
        }

        public int TotalBytesRead
        {
            get
            {
                return this._TotalBytesRead;
            }
        }
    }
}