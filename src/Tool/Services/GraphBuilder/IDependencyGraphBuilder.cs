using System.Threading.Tasks;
using PackProject.Tool.Models;

namespace PackProject.Tool.Services.GraphBuilder
{
    public interface IDependencyGraphBuilder
    {
        Task<DependencyGraph> BuildAsync();
    }
}