using System.Threading.Tasks;
using PackProject.Tool.Options;

namespace PackProject.Tool.Services.DotNetPack
{
    public interface IDotNetPack
    {
        Task PackAsync(string projectPath, PackOptions options);
    }
}