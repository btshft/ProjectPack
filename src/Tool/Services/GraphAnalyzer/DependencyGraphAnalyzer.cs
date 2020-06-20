using PackProject.Tool.Models;

namespace PackProject.Tool.Services.GraphAnalyzer
{
    public class DependencyGraphAnalyzer : IDependencyGraphAnalyzer
    {
        public DependencyGraphAnalysis Analyze(DependencyGraph graph)
        {
            return new DependencyGraphAnalysis(graph.Projects.Keys);
        }
    }
}