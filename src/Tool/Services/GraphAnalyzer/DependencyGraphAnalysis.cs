using System;
using System.Collections.Generic;

namespace PackProject.Tool.Services.GraphAnalyzer
{
    public class DependencyGraphAnalysis
    {
        public DependencyGraphAnalysis(
            IEnumerable<ProjectDefinition> projects, 
            IEnumerable<string> remoteSources)
        {
            PackageSources = new HashSet<string>(remoteSources, StringComparer.InvariantCultureIgnoreCase);
            Projects = new HashSet<ProjectDefinition>(projects, ProjectDefinition.PathComparer);
        }

        public IReadOnlyCollection<ProjectDefinition> Projects { get; } 

        public IReadOnlyCollection<string> PackageSources { get; }

        public static DependencyGraphAnalysis Empty { get; } = new DependencyGraphAnalysis(
            Array.Empty<ProjectDefinition>(), 
            Array.Empty<string>());
    }
}