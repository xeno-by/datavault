using System;

namespace DataVault.Core.Helpers.Assertions
{
    public class AssertionFailedException : Exception
    {
        public AssertionFailedException() {
        }

        public AssertionFailedException(String message)
            : base(message) 
        {
        }

        public AssertionFailedException(String message, Exception innerException)
            : base(message, innerException) 
        {
        }
    }
}