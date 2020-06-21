using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PackProject.Tool.Exceptions;
using PackProject.Tool.Options;
using PackProject.Tool.Services.CommandLine;
using PackProject.Tool.Services.ExecutionContext;
using PackProject.Tool.Services.GraphAnalyzer;
using PackProject.Tool.Services.GraphBuilder;
using PackProject.Tool.Services.GraphProcessor;

namespace PackProject.Tool
{
    public class Executor
    {
        private readonly ICommandArgsExtractor _argsExtractor;
        private readonly IExecutionContextAccessor _contextAccessor;
        private readonly IDependencyGraphBuilder _graphBuilder;
        private readonly IDependencyGraphAnalyzer _graphAnalyzer;
        private readonly IEnumerable<IDependencyGraphProcessor> _processors;
        private readonly ILogger<Executor> _logger;

        public Executor(
            ICommandArgsExtractor argsExtractor,
            IExecutionContextAccessor contextAccessor,
            IDependencyGraphBuilder graphBuilder,
            IDependencyGraphAnalyzer graphAnalyzer,
            IEnumerable<IDependencyGraphProcessor> processors, 
            ILogger<Executor> logger)
        {
            _argsExtractor = argsExtractor;
            _contextAccessor = contextAccessor;
            _graphBuilder = graphBuilder;
            _graphAnalyzer = graphAnalyzer;
            _processors = processors;
            _logger = logger;
        }

        public async Task<int> ExecuteAsync(
            string configuration = null,
            bool force = false,
            bool includeSource = false,
            bool interactive = false,
            bool noBuild = false,
            bool noRestore = false,
            bool nologo = false,
            string output = null,
            string runtime = null,
            bool serviceable = false,
            string verbosity = null,
            string versionSuffix = null,
            bool parallel = false,
            string outputGraph = null,
            bool warnDowngrade = false,
            bool disallowDowngrade = false)
        {
            void InitContext()
            {
                var parameters = _argsExtractor.GetParameters();
                var projectPath = _argsExtractor.GetProjectPath();

                if (string.IsNullOrEmpty(projectPath))
                    throw new InvalidInputException(exitCode: 1, "Project path is required");

                var options = new ProjectPackOptions
                {
                    Parameters = parameters,

                    // dotnet pack
                    ProjectPath = projectPath,
                    Configuration = configuration,
                    Force = force,
                    IncludeSource = includeSource,
                    Interactive = interactive,
                    NoBuild = noBuild,
                    NoRestore = noRestore,
                    NoLogo = nologo,
                    Output = output,
                    Runtime = runtime,
                    Serviceable = serviceable,
                    Verbosity = verbosity,
                    VersionSuffix = versionSuffix,

                    // dotnet pack-project
                    Parallel = parallel,
                    OutputGraph = outputGraph,
                    WarnDowngrade = warnDowngrade,
                    DisallowDowngrade = disallowDowngrade
                };

                _contextAccessor.Context = new ExecutionContext
                {
                    Options = options
                };
            }

            InitContext();

            var dependencyGraph = await _graphBuilder.BuildAsync();
            var result = _graphAnalyzer.Analyze(dependencyGraph);

            foreach (var processor in _processors)
            {
                _logger.LogInformation("Executing {Processor}", processor.GetType().Name);
                await processor.ProcessAsync(result, dependencyGraph);
            }

            return 0;
        }
    }
}