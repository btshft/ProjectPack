using System;
using System.Runtime.Serialization;

namespace PackProject.Tool.Services.Runner
{
    [Serializable]
    public class CommandRunnerException : Exception
    {
        public int ExitCode { get; private set; }

        public CommandRunnerException(string message, int exitCode)
            : base($"Runner failed with code '{exitCode}': {message}")
        {
            ExitCode = exitCode;
        }

        /// <inheritdoc />
        protected CommandRunnerException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}