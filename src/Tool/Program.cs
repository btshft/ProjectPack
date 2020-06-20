using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PackProject.Tool.Services.CommandLine;
using PackProject.Tool.Services.ExecutionContext;
using PackProject.Tool.Services.GraphAnalizer;
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
                .AddSingleton<ICommandLineArgsFilter>(new CommandLineArgsFilter(args));

            serviceCollection.AddSingleton(sp =>
            {
                return LoggerFactory.Create(cfg =>
                {
                    cfg.AddConsole();
                    cfg.AddFilter(_ => true);
                });
            });

            serviceCollection.AddTransient(typeof(ILogger<>), typeof(Logger<>));

            return serviceCollection.BuildServiceProvider();
        }

        public static async Task<int> Main(string[] args)
        {
            await using var provider = BuildServiceProvider(args);

            var command = new RootCommand("Generates NuGet package for specified project including all project dependencies as separate NuGet packages with same version.");

            var configurationOption = new Option<string>(
                aliases: new[] { "--configuration", "-c" },
                description: "Defines the build configuration.");

            var outputOption = new Option<string>(
                aliases: new[] { "--output", "-o" },
                description: "Places the built packages in the directory specified.");

            var debugOption = new Option<bool>(
                aliases: new[] { "--debug" },
                description: "Debug mode.");

            var parallelOption = new Option<bool>(
                aliases: new[] { "--parallel" },
                description: "Run in parallel.");

            var parser = new CommandLineBuilder(command)
                .AddOption(configurationOption)
                .AddOption(outputOption)
                .AddOption(debugOption)
                .AddOption(parallelOption)
                .CancelOnProcessTermination()
                .UseExceptionHandler()
                .RegisterWithDotnetSuggest()
                .UseSuggestDirective()
                .UseHelp()
                .UseParseDirective()
                .Build();

            // ReSharper disable once AccessToDisposedClosure
            var builder = provider.GetRequiredService<IDependencyGraphBuilder>();
            var analyzer = provider.GetRequiredService<IDependencyGraphAnalyzer>();
            var processor = provider.GetRequiredService<IDependencyGraphProcessor>();
            var contextAccessor = provider.GetRequiredService<IExecutionContextAccessor>();
            var argsFilter = provider.GetRequiredService<ICommandLineArgsFilter>();

            command.Handler = CommandHandler.Create<string, string, bool, bool>(
                async (configuration, output, debug, parallel) =>
            {
                try
                {
                    contextAccessor.Context = new ExecutionContext
                    {
                        Configuration = configuration,
                        IsDebugMode = debug,
                        Output = output,
                        ProjectPath = argsFilter.GetProjectPath(),
                        RunParallel = parallel
                    };

                    var dependencyGraph = await builder.BuildAsync();
                    var result = analyzer.Analyze(dependencyGraph);
                    await processor.ProcessAsync(result, dependencyGraph);

                    return 0;
                }
                catch (CommandRunnerException e)
                {
                    return e.ExitCode;
                }
            });

            try
            {
                return await parser.InvokeAsync(args);
            }
            catch (CommandRunnerException ne)
            {
                return ne.ExitCode;
            }
        }
    }
}