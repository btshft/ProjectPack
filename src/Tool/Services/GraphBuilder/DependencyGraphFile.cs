using System;
using System.IO;

namespace PackProject.Tool.Services.GraphBuilder
{
    public class DependencyGraphFile : IDisposable
    {
        public DependencyGraphFile(string name)
        {
            Path = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Path { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!File.Exists(Path)) 
                return;

            try
            {
                File.Delete(Path);
            }
            catch
            {
                // We dont care
            }
        }

        public static DependencyGraphFile Create()
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetTempFileName());
            return new DependencyGraphFile(path);
        }
    }
}