using System.Collections.Generic;

namespace PackProject.Tool.Services.GraphAnalyzer
{
    public class ProjectDefinition
    {
        public static IEqualityComparer<ProjectDefinition> PathComparer { get; } = new PathEqualityComparer();

        public string Path { get; set; }

        public string Version { get; set; }

        public string PackageName { get; set; }

        private sealed class PathEqualityComparer : IEqualityComparer<ProjectDefinition>
        {
            public bool Equals(ProjectDefinition x, ProjectDefinition y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Path == y.Path;
            }

            public int GetHashCode(ProjectDefinition obj)
            {
                return (obj.Path != null ? obj.Path.GetHashCode() : 0);
            }
        }
    }
}