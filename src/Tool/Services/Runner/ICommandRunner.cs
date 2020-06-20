using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackProject.Tool.Services.Runner
{
    public interface ICommandRunner
    {
        public Task RunAsync(string fileName, string command, IEnumerable<string> arguments);
    }
}
