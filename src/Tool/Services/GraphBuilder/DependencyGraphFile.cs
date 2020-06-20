using System;
using System.IO;

namespace PackProject.Tool.Services.GraphBuilder
{
    public class DependencyGraphFile : IDisposable
    {
        public DependencyGraphFile(string name, bool autoDelete)
        {
            FilePath = name ?? throw new ArgumentNullException(nameof(name));
            AutoDelete = autoDelete;
        }

        public string FilePath { get; }

        public bool AutoDelete { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!AutoDelete || !File.Exists(FilePath)) 
                return;

            try
            {
                File.Delete(FilePath);
            }
            catch
            {
                // We dont care
            }
        }

        public static DependencyGraphFile Create()
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            return new DependencyGraphFile(path, autoDelete: true);
        }

        public static DependencyGraphFile Create(string path, bool autoDelete = true)
        {
            if (path == null) 
                throw new ArgumentNullException(nameof(path));

            var fullPath = Path.GetFullPath(path);
            if (!File.Exists(fullPath))
                File.Create(fullPath).Dispose();

            return new DependencyGraphFile(fullPath, autoDelete);
        }
    }
}