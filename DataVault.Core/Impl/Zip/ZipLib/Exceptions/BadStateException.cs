using System;
using System.Runtime.Serialization;

namespace DataVault.Core.Impl.Zip.ZipLib.Exceptions
{
    [Serializable]
    internal class BadStateException : ZipException
    {
        public BadStateException()
        {
        }

        public BadStateException(String message)
            : base(message)
        {
        }

        protected BadStateException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        public BadStateException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}