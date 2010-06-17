using System;
using System.Runtime.Serialization;

namespace DataVault.Core.Impl.Zip.ZipLib.Exceptions
{
    [Serializable]
    internal class BadReadException : ZipException
    {
        public BadReadException()
        {
        }

        public BadReadException(String message)
            : base(message)
        {
        }

        protected BadReadException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        public BadReadException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}