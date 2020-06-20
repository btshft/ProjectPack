using System.Collections.Generic;
using System.Text.Json;

namespace PackProject.Tool.Models
{
    public class DependencyGraph
    {
        public Dictionary<string, None> Restore { get; set; }
            = new Dictionary<string, None>();

        public Dictionary<string, ProjectSpecification> Projects { get; set; }
            = new Dictionary<string, ProjectSpecification>();

        /// <inheritdoc />
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true,
                IgnoreNullValues = true
            });
        }
    }
}