using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using PackProject.Tool.Models;
using PackProject.Tool.Services.ExecutionContext;

namespace PackProject.Tool.Services.GraphAnalyzer
{
    public class DependencyGraphAnalyzer : IDependencyGraphAnalyzer
    {
        private readonly IExecutionContextAccessor _contextAccessor;
        private readonly ILogger<DependencyGraphAnalyzer> _logger;

        public DependencyGraphAnalyzer(IExecutionContextAccessor contextAccessor, ILogger<DependencyGraphAnalyzer> logger)
        {
            _contextAccessor = contextAccessor;
            _logger = logger;
        }

        public DependencyGraphAnalysis Analyze(DependencyGraph graph)
        {
            if (graph.Projects == null || graph.Projects.Count < 1)
                return DependencyGraphAnalysis.Empty;

            var options = _contextAccessor.Context.Options;
            var sources = new List<string>();

            var (_, project) = graph.Projects.Last();
            if (project.Restore?.Sources != null)
            {
                sources.AddRange(project.Restore.Sources.Keys);
            }

            var projects = new List<ProjectDefinition>();
            foreach (var projectPath in graph.Projects.Keys)
            {
                if (graph.Projects.TryGetValue(projectPath, out var projectRef))
                {
                    projects.Add(new ProjectDefinition
                    {
                        PackageName = projectRef.Restore.ProjectName,
                        Path = projectPath,
                        Version = projectRef.Version
                    });
                }
                else
                {
                    _logger.LogError($"Project '{options.ProjectPath}' not found in dependency graph");
                }
            }
            return new DependencyGraphAnalysis(projects, sources);
        }
    }
}