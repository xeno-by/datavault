using System;
using System.IO;
using System.Text;
using DataVault.Core.Impl.Zip.ZipLib.Exceptions;

namespace DataVault.Core.Impl.Zip.ZipLib
{
    internal static class ZipSharedUtilities
    {
        // Fields
        private static Encoding ibm437 = Encoding.GetEncoding("IBM437");
        private static Encoding utf8 = Encoding.GetEncoding("UTF-8");

        private static int _ReadFourBytes(Stream s, String message)
        {
            byte[] block = new byte[4];
            if (s.Read(block, 0, block.Length) != block.Length)
            {
                throw new BadReadException(message);
            }
            return ((((((block[3] * 0x100) + block[2]) * 0x100) + block[1]) * 0x100) + block[0]);
        }

        internal static int DateTimeToPacked(DateTime time)
        {
            ushort packedDate = (ushort) (((time.Day & 0x1f) | ((time.Month << 5) & 480)) | (((time.Year - 0x7bc) << 9) & 0xfe00));
            ushort packedTime = (ushort) ((((time.Second / 2) & 0x1f) | ((time.Minute << 5) & 0x7e0)) | ((time.Hour << 11) & 0xf800));
            return ((packedDate << 0x10) | packedTime);
        }

        internal static long FindSignature(Stream stream, int SignatureToFind)
        {
            long startingPosition = stream.Position;
            int BATCH_SIZE = 0x10000;
            byte[] targetBytes = new byte[] { (byte) (SignatureToFind >> 0x18), (byte) ((SignatureToFind & 0xff0000) >> 0x10), (byte) ((SignatureToFind & 0xff00) >> 8), (byte) (SignatureToFind & 0xff) };
            byte[] batch = new byte[BATCH_SIZE];
            int n = 0;
            bool success = false;
        Label_0050:
            n = stream.Read(batch, 0, batch.Length);
            if (n != 0)
            {
                for (int i = 0; i < n; i++)
                {
                    if (batch[i] == targetBytes[3])
                    {
                        long curPosition = stream.Position;
                        stream.Seek((long) (i - n), SeekOrigin.Current);
                        success = ReadSignature(stream) == SignatureToFind;
                        if (success)
                        {
                            break;
                        }
                        stream.Seek(curPosition, SeekOrigin.Begin);
                    }
                }
                if (!success)
                {
                    goto Label_0050;
                }
            }
            if (!success)
            {
                stream.Seek(startingPosition, SeekOrigin.Begin);
                return -1L;
            }
            return ((stream.Position - startingPosition) - 4L);
        }

        internal static bool HighBytes(byte[] buffer)
        {
            if (buffer != null)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    if ((buffer[i] & 0x80) == 0x80)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static DateTime PackedToDateTime(int packedDateTime)
        {
            short packedTime = (short) (packedDateTime & 0xffff);
            short packedDate = (short) ((packedDateTime & 0xffff0000L) >> 0x10);
            int year = 0x7bc + ((packedDate & 0xfe00) >> 9);
            int month = (packedDate & 480) >> 5;
            int day = packedDate & 0x1f;
            int hour = (packedTime & 0xf800) >> 11;
            int minute = (packedTime & 0x7e0) >> 5;
            int second = (packedTime & 0x1f) * 2;
            if (second >= 60)
            {
                minute++;
                second = 0;
            }
            if (minute >= 60)
            {
                hour++;
                minute = 0;
            }
            if (hour >= 0x18)
            {
                day++;
                hour = 0;
            }
            DateTime d = DateTime.Now;
            try
            {
                d = new DateTime(year, month, day, hour, minute, second, 0);
            }
            catch (ArgumentOutOfRangeException ex1)
            {
                throw new ZipException("Bad date/time format in the zip file.", ex1);
            }
            return d;
        }

        internal static int ReadInt(Stream s)
        {
            return _ReadFourBytes(s, "Could not read block - no data!");
        }

        internal static int ReadSignature(Stream s)
        {
            return _ReadFourBytes(s, "Could not read signature - no data!");
        }

        public static DateTime RoundToEvenSecond(DateTime source)
        {
            if ((source.Second % 2) == 1)
            {
                source += new TimeSpan(0, 0, 1);
            }
            return new DateTime(source.Year, source.Month, source.Day, source.Hour, source.Minute, source.Second);
        }

        internal static String StringFromBuffer(byte[] buf, int maxlength)
        {
            return StringFromBuffer(buf, maxlength, ibm437);
        }

        internal static String StringFromBuffer(byte[] buf, int maxlength, Encoding encoding)
        {
            return encoding.GetString(buf);
        }

        internal static byte[] ToByteArray(this String value)
        {
            return value.ToByteArray(ibm437);
        }

        internal static byte[] ToByteArray(this String value, Encoding encoding)
        {
            return encoding.GetBytes(value);
        }

        public static String TrimVolumeAndSwapSlashes(String pathName)
        {
            if (String.IsNullOrEmpty(pathName))
            {
                return pathName;
            }
            if (pathName.Length < 2)
            {
                return pathName.Replace('\\', '/');
            }
            return (((pathName[1] == ':') && (pathName[2] == '\\')) ? pathName.Substring(3) : pathName).Replace('\\', '/');
        }

        internal static String Utf8StringFromBuffer(byte[] buf, int maxlength)
        {
            return StringFromBuffer(buf, maxlength, utf8);
        }

        internal static byte[] Utf8StringToByteArray(String value)
        {
            return value.ToByteArray(utf8);
        }

        public static Stream ExtractEager(this ZipEntry ze)
        {
            if (ze == null) return null;
            var ms = new MemoryStream();
            ze.Extract(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public static Func<Stream> ExtractLazy(this ZipEntry ze)
        {
            return () => ze.ExtractEager();
        }
    }
}