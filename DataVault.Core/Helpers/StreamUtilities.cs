using System;
using System.IO;

namespace DataVault.Core.Helpers
{
    public static class StreamUtilities
    {
        public static Stream AsStream(this String s)
        {
            if (s == null)
            {
                return null;
            }
            else
            {
                var m = new MemoryStream();

                var sw = new StreamWriter(m);
                sw.Write(s ?? String.Empty);
                sw.Flush();

                m.Seek(0, SeekOrigin.Begin);
                return m;
            }
        }

        // note. this method must return a clone of the stream
        // but never the same reference, since there exist logics
        // that depends on this behavior (ZipFile::AddFileStream)

        public static Stream CacheInMemory(this Stream s)
        {
            if (s == null)
            {
                return null;
            }
            else
            {
                lock (s)
                {
                    var m = new MemoryStream();
                    var originalPos = s.Position;

                    try
                    {
                        int b;
                        while ((b = s.ReadByte()) != -1)
                        {
                            m.WriteByte((byte)b);
                        }

                        return m;
                    }
                    finally
                    {
                        s.Seek(originalPos, SeekOrigin.Begin);
                        m.Seek(0, SeekOrigin.Begin);
                    }
                }
            }
        }

        public static Func<Stream> AsLazyStream(this String s)
        {
            return () => s.AsStream();
        }

        public static String AsString(this Stream s)
        {
            if(s == null)
            {
                return null;
            }
            else
            {
                lock (s)
                {
                    var originalPos = s.Position;

                    try
                    {
                        return new StreamReader(s).ReadToEnd();
                    }
                    finally
                    {
                        s.Seek(originalPos, SeekOrigin.Begin);
                    }
                }
            }
        }

        public static byte[] AsByteArray(this Stream s)
        {
            if (s == null)
            {
                return null;
            }
            else
            {
                lock (s)
                {
                    var originalPos = s.Position;

                    try
                    {
                        var readBuffer = new byte[4096];

                        var totalBytesRead = 0;
                        int bytesRead;

                        while ((bytesRead = s.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                        {
                            totalBytesRead += bytesRead;

                            if (totalBytesRead == readBuffer.Length)
                            {
                                var nextByte = s.ReadByte();
                                if (nextByte != -1)
                                {
                                    var temp = new byte[readBuffer.Length * 2];
                                    Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                                    Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                                    readBuffer = temp;
                                    totalBytesRead++;
                                }
                            }
                        }

                        var buffer = readBuffer;
                        if (readBuffer.Length != totalBytesRead)
                        {
                            buffer = new byte[totalBytesRead];
                            Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                        }

                        return buffer;
                    }
                    finally
                    {
                        s.Seek(originalPos, SeekOrigin.Begin);
                    }
                }
            }
        }

        public static byte[] WriteToByteArray(this Stream s)
        {
            if (s == null)
            {
                return null;
            }
            else
            {
                lock (s)
                {
                    var originalPos = s.Position;

                    try
                    {
                        var readBuffer = new byte[4096];

                        var totalBytesRead = 0;
                        int bytesRead;

                        while ((bytesRead = s.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                        {
                            totalBytesRead += bytesRead;

                            if (totalBytesRead == readBuffer.Length)
                            {
                                var nextByte = s.ReadByte();
                                if (nextByte != -1)
                                {
                                    var temp = new byte[readBuffer.Length * 2];
                                    Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                                    Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                                    readBuffer = temp;
                                    totalBytesRead++;
                                }
                            }
                        }

                        var buffer = readBuffer;
                        if (readBuffer.Length != totalBytesRead)
                        {
                            buffer = new byte[totalBytesRead];
                            Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                        }

                        return buffer;
                    }
                    finally
                    {
                        s.Seek(originalPos, SeekOrigin.Begin);
                    }
                }
            }
        }

        public static void WriteToFile(this Stream stream, String fileName)
        {
            File.WriteAllBytes(fileName, stream.WriteToByteArray() ?? new byte[0]);
        }

        public static String DumpToTempFile(this Stream stream)
        {
            var tempFile = Path.GetTempFileName();
            stream.WriteToFile(tempFile);
            return tempFile;
        }

        public static void ReadFromByteArray(this Stream s, byte[] bytes)
        {
            s.Write(bytes, 0, bytes.Length);
        }

        public static void ReadFromFile(this Stream stream, String fileName)
        {
            var bytes = File.ReadAllBytes(fileName);
            stream.ReadFromByteArray(bytes);
        }
    }
}