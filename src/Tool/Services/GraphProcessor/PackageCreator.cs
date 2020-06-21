using System.Linq;
using System.Threading.Tasks;
using PackProject.Tool.Models;
using PackProject.Tool.Services.DotNetPack;
using PackProject.Tool.Services.ExecutionContext;
using PackProject.Tool.Services.GraphAnalyzer;

namespace PackProject.Tool.Services.GraphProcessor
{
    public class PackageCreator : IDependencyGraphProcessor
    {
        private readonly IExecutionContextAccessor _contextAccessor;
        private readonly IDotNetPack _dotNetPack;

        public PackageCreator(
            IExecutionContextAccessor contextAccessor, IDotNetPack dotNetPack)
        {
            _contextAccessor = contextAccessor;
            _dotNetPack = dotNetPack;
        }

        /// <inheritdoc />
        public async Task ProcessAsync(DependencyGraphAnalysis analysis, DependencyGraph graph)
        {
            var options = _contextAccessor.Context.Options;

            if (options.Parallel)
            {
                var runTasks = analysis.Projects.Select(
                    project => _dotNetPack.PackAsync(project.Path, options));

                await Task.WhenAll(runTasks);
            }
            else
            {
                foreach (var project in analysis.Projects)
                {
                    await _dotNetPack.PackAsync(project.Path, options);
                }
            }
        }
    }
}