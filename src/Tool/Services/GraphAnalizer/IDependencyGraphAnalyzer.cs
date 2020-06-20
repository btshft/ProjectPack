using PackProject.Tool.Models;

namespace PackProject.Tool.Services.GraphAnalizer
{
    public interface IDependencyGraphAnalyzer
    {
        DependencyGraphAnalysis Analyze(DependencyGraph graph);
    }
}