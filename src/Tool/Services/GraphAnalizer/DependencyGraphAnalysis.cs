using System;
using System.Collections.Generic;

namespace PackProject.Tool.Services.GraphAnalizer
{
    public class DependencyGraphAnalysis
    {
        public DependencyGraphAnalysis(IEnumerable<string> projects)
        {
            Projects = new HashSet<string>(projects, StringComparer.InvariantCultureIgnoreCase);
        }

        public IReadOnlyCollection<string> Projects { get; }
    }
}