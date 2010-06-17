using System;
using System.Runtime.Serialization;

namespace DataVault.Core.Impl.Zip.ZipLib.Exceptions
{
    [Serializable]
    internal class BadCrcException : ZipException
    {
        public BadCrcException()
        {
        }

        public BadCrcException(String message)
            : base(message)
        {
        }

        protected BadCrcException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        public BadCrcException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}