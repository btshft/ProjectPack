using System.Collections.Generic;

namespace PackProject.Tool.Models
{
    public class RestoreSpecification
    {
        public string ProjectUniqueName { get; set; }

        public string ProjectName { get; set; }

        public string ProjectPath { get; set; }

        public string PackagesPath { get; set; }

        public string OutputPath { get; set; }

        public string ProjectStyle { get; set; }

        public List<string> OriginalTargetFrameworks { get; set; } = new List<string>();

        public Dictionary<string, None> Sources { get; set; } = new Dictionary<string, None>();

        public Dictionary<string, FrameworkSpecification> Frameworks { get; set; } 
            = new Dictionary<string, FrameworkSpecification>();
    }
}