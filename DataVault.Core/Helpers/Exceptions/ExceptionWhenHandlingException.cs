using System;

namespace DataVault.Core.Helpers.Exceptions
{
    public class ExceptionWhenHandlingException : Exception
    {
        public Exception OriginalException { get; private set; }
        public Exception CatchBlockException { get; private set; }

        public ExceptionWhenHandlingException(Exception originalException, Exception catchBlockException)
            : base(null, originalException)
        {
            OriginalException = originalException;
            CatchBlockException = catchBlockException;
        }

        // todo. implement this
        private bool RunningUnderResharperNUnit { get { return true; } }

        public override string Message
        {
            get
            {
                if (RunningUnderResharperNUnit)
                {
                    return String.Format(
                        "An exception has been thrown when handling an exception.{0}" +
                        "Original exception is stored in an InnerException, and is not included in the message (Resharper will show it above).{0}" +
                        "Here's an exception thrown inside a handler block (I wish Resharper could parse its stack trace as well):{0}" +
                        "**************************************************{0}" +
                        "{1}{0}" +
                        "**************************************************{0}",
                        Environment.NewLine,
                        CatchBlockException);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
