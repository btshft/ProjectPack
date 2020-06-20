using System;
using System.Runtime.Serialization;

namespace PackProject.Tool.Exceptions
{
    [Serializable]
    public class InvalidInputException : NonZeroExitCodeException
    {
        public string Reason { get;  }

        /// <inheritdoc />
        public InvalidInputException(int exitCode, string reason)
            : base(reason, exitCode)
        {
            Reason = reason;
        }

        /// <inheritdoc />
        protected InvalidInputException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}