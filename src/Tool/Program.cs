using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PackProject.Tool.Exceptions;
using PackProject.Tool.Services.CommandLine;
using PackProject.Tool.Services.DotNetPack;
using PackProject.Tool.Services.ExecutionContext;
using PackProject.Tool.Services.GraphAnalyzer;
using PackProject.Tool.Services.GraphBuilder;
using PackProject.Tool.Services.GraphProcessor;
using PackProject.Tool.Services.Runner;

namespace PackProject.Tool
{
    public static class Program
    {
        private static ServiceProvider BuildServiceProvider(string[] args)
        {
            var serviceCollection = new ServiceCollection()
                .AddSingleton<IDependencyGraphAnalyzer, DependencyGraphAnalyzer>()
                .AddSingleton<IDependencyGraphBuilder, DependencyGraphBuilder>()
                .AddSingleton<IDependencyGraphProcessor, DependencyGraphProcessor>()
                .AddSingleton<IExecutionContextAccessor, ExecutionContextAccessor>()
                .AddSingleton<ICommandRunner, CommandRunner>()
                .AddSingleton<IDotNetPack, DotNetPack>()
                .AddSingleton<ICommandArgsExtractor>(new CommandArgsExtractor(args))
                .AddSingleton<Executor>();

            serviceCollection.AddSingleton(sp =>
            {
                var extractor = sp.GetRequiredService<ICommandArgsExtractor>();

                return LoggerFactory.Create(cfg =>
                {
                    var verbosity = extractor.GetVerbosity();
                    if (verbosity != null)
                    {
                        var logLevel = verbosity.ToLowerInvariant() switch
                        {
                            "q" => LogLevel.Error,
                            "quiet" => LogLevel.Error,
                            "m" => LogLevel.Warning,
                            "minimal" => LogLevel.Warning,
                            "n" => LogLevel.Information,
                            "normal" => LogLevel.Information,
                            "d" => LogLevel.Debug,
                            "detailed" => LogLevel.Debug,
                            "diag" => LogLevel.Debug,
                            "diagnostic" => LogLevel.Debug,
                            _ => LogLevel.Error
                        };

                        cfg.SetMinimumLevel(logLevel);
                    }

                    cfg.AddConsole();
                });
            });

            serviceCollection.AddTransient(typeof(ILogger<>), typeof(Logger<>));

            return serviceCollection.BuildServiceProvider();
        }

        public static async Task<int> Main(string[] args)
        {
            await using var provider = BuildServiceProvider(args);

            var command = new RootCommand("Generates NuGet package for specified project including all project dependencies as separate NuGet packages with same version.");
            var parser = new CommandLineBuilder(command)
                .AddDotnetPackOptions()
                .AddCustomOptions()
                .CancelOnProcessTermination()
                .UseHelp()
                .Build();

            var executor = provider.GetRequiredService<Executor>();
            var method = executor.GetType().GetTypeInfo()
                .GetDeclaredMethod(nameof(Executor.ExecuteAsync));

            command.Handler = CommandHandler.Create(method!, executor);

            try
            {
                return await parser.InvokeAsync(args);
            }
            catch (InvalidInputException ie)
            {
                await Console.Error.WriteLineAsync("Execution terminated because of invalid input");

                if (ie.Reason != null)
                {
                    await Console.Error.WriteLineAsync("Reason:");
                    await Console.Error.WriteLineAsync(ie.Reason);
                }

                return ie.ExitCode;
            }
            catch (NonZeroExitCodeException ne)
            {
                return ne.ExitCode;
            }
        }
    }
}