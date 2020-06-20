namespace PackProject.Tool.Models
{
    public class ProjectSpecification
    {
        public string Version { get; set; }

        public RestoreSpecification Restore { get; set; }
    }
}