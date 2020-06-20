using System.Threading.Tasks;
using PackProject.Tool.Models;
using PackProject.Tool.Services.GraphAnalyzer;

namespace PackProject.Tool.Services.GraphProcessor
{
    public interface IDependencyGraphProcessor
    {
        Task ProcessAsync(DependencyGraphAnalysis analysis, DependencyGraph graph);
    }
}