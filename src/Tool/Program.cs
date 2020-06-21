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
using PackProject.Tool.Extensions;
using PackProject.Tool.Services.CommandLine;
using PackProject.Tool.Services.DotNetPack;
using PackProject.Tool.Services.ExecutionContext;
using PackProject.Tool.Services.GraphAnalyzer;
using PackProject.Tool.Services.GraphBuilder;
using PackProject.Tool.Services.GraphProcessor;
using PackProject.Tool.Services.Runner;
using Serilog;
using Serilog.Extensions.Logging;

namespace PackProject.Tool
{
    public static class Program
    {
        private static ServiceProvider BuildServiceProvider(string[] args)
        {
            var serviceCollection = new ServiceCollection()
                .AddSingleton<IDependencyGraphAnalyzer, DependencyGraphAnalyzer>()
                .AddSingleton<IDependencyGraphBuilder, DependencyGraphBuilder>()
                .AddSingleton<IDependencyGraphProcessor, PackageDowngradeScanner>()
                .AddSingleton<IDependencyGraphProcessor, PackageCreator>()
                .AddSingleton<IExecutionContextAccessor, ExecutionContextAccessor>()
                .AddSingleton<ICommandRunner, CommandRunner>()
                .AddSingleton<IDotNetPack, DotNetPack>()
                .AddSingleton<ICommandArgsExtractor>(new CommandArgsExtractor(args))
                .AddSingleton<Executor>();

            serviceCollection.AddSingleton<ILoggerFactory>(sp =>
            {
                var extractor = sp.GetRequiredService<ICommandArgsExtractor>();

                var level = SerilogHelper.GetLevel(extractor.GetVerbosity());
                var logger = new LoggerConfiguration()
                    .WriteTo.Console(level, outputTemplate: "{NewLine}[{Level}] <{SourceContext:l}> {NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
                    .CreateLogger();

                return new SerilogLoggerFactory(logger, dispose: true);
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