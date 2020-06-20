using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PackProject.Tool.Exceptions;

namespace PackProject.Tool.Services.Runner
{
    public class CommandRunner : ICommandRunner
    {
        private readonly ILogger<CommandRunner> _logger;

        public CommandRunner(ILogger<CommandRunner> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task RunAsync(string fileName, string command, IEnumerable<string> arguments)
        {
            static Task RunAsync(Process process)
            {
                var tcs = new TaskCompletionSource<object>();

                process.Exited += (sender, eventArgs) =>
                {
                    tcs.SetResult(default);
                };

                process.EnableRaisingEvents = true;
                process.Start();

                return tcs.Task;
            }

            var args = string.Join(" ", arguments.Where(a => a != null));
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(fileName, $"{command} {args}")
                {
                    UseShellExecute = false,
                    RedirectStandardError = false,
                    RedirectStandardOutput = false
                }
            };

            _logger.LogInformation($"Preparing to run: {fileName} {command} {args}");

            await RunAsync(process);
            if (process.ExitCode != 0)
                throw new NonZeroExitCodeException($"{command} {args}", process.ExitCode);
        }
    }
}