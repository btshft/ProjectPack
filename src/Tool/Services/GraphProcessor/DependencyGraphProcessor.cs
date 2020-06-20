using System;
using System.Linq;
using System.Threading.Tasks;
using PackProject.Tool.Models;
using PackProject.Tool.Services.CommandLine;
using PackProject.Tool.Services.ExecutionContext;
using PackProject.Tool.Services.GraphAnalizer;
using PackProject.Tool.Services.Runner;

namespace PackProject.Tool.Services.GraphProcessor
{
    public class DependencyGraphProcessor : IDependencyGraphProcessor
    {
        private readonly ICommandLineArgsFilter _argsFilter;
        private readonly IExecutionContextAccessor _contextAccessor;
        private readonly ICommandRunner _commandRunner;

        public DependencyGraphProcessor(
            IExecutionContextAccessor contextAccessor, 
            ICommandRunner commandRunner,
            ICommandLineArgsFilter argsFilter)
        {
            _contextAccessor = contextAccessor;
            _commandRunner = commandRunner;
            _argsFilter = argsFilter;
        }

        /// <inheritdoc />
        public async Task ProcessAsync(DependencyGraphAnalysis analysis, DependencyGraph graph)
        {
            var context = _contextAccessor.Context;
            var bypassArgs = _argsFilter.GetBypass();

            if (context.ProjectPath != null)
            {
                var projectIndex = Array.IndexOf(bypassArgs, context.ProjectPath);
                if (projectIndex > -1)
                {
                    // Remove original path as we'll replace it with custom path of dependency
                    bypassArgs[projectIndex] = null;
                }
            }

            if (context.RunParallel)
            {
                var runTasks = analysis.Projects.Select(s =>
                {
                    var args = new[] { s }.Concat(bypassArgs);
                    return _commandRunner.RunAsync("dotnet", "pack", args);
                });

                await Task.WhenAll(runTasks);
            }
            else
            {
                foreach (var project in analysis.Projects)
                {
                    var args = new[] { project }.Concat(bypassArgs);
                    await _commandRunner.RunAsync("dotnet", "pack", args);
                }
            }
        }
    }
}