using PackProject.Tool.Models;

namespace PackProject.Tool.Services.GraphAnalizer
{
    public class DependencyGraphAnalyzer : IDependencyGraphAnalyzer
    {
        public DependencyGraphAnalysis Analyze(DependencyGraph graph)
        {
            return new DependencyGraphAnalysis(graph.Projects.Keys);
        }
    }
}