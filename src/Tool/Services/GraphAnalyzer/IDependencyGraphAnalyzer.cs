using PackProject.Tool.Models;

namespace PackProject.Tool.Services.GraphAnalyzer
{
    public interface IDependencyGraphAnalyzer
    {
        DependencyGraphAnalysis Analyze(DependencyGraph graph);
    }
}