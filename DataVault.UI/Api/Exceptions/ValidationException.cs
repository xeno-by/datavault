using System;

namespace DataVault.UI.Api.Exceptions
{
    public class ValidationException : ApplicationException
    {
        public ValidationException(String message, params Object[] args)
            : base(String.Format(message, args)) 
        {
        }

        public ValidationException(String message, Exception inner, params Object[] args)
            : base(String.Format(message, args), inner)
        {
        }
    }
}