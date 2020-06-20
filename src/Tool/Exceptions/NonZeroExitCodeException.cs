using System;
using System.Runtime.Serialization;

namespace PackProject.Tool.Exceptions
{
    [Serializable]
    public class NonZeroExitCodeException : Exception
    {
        public int ExitCode { get; private set; }

        public NonZeroExitCodeException(string message, int exitCode)
            : base($"Runner failed with code '{exitCode}': {message}")
        {
            ExitCode = exitCode;
        }

        /// <inheritdoc />
        protected NonZeroExitCodeException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}