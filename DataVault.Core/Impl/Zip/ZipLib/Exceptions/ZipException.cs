using System;
using System.Runtime.Serialization;

namespace DataVault.Core.Impl.Zip.ZipLib.Exceptions
{
    [Serializable]
    public class ZipException : Exception
    {
        public ZipException()
        {
        }

        public ZipException(String message)
            : base(message)
        {
        }

        protected ZipException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        public ZipException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}