using System.Collections.Generic;

namespace PackProject.Tool.Models
{
    public class FrameworkSpecification
    {
        public Dictionary<string, ProjectReferenceSpecification> ProjectReferences { get; set; } 
            = new Dictionary<string, ProjectReferenceSpecification>();
    }
}