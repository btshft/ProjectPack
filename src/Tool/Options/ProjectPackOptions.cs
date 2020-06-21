namespace PackProject.Tool.Options
{
    public class ProjectPackOptions : PackOptions
    {
        public string OutputGraph { get; set; }

        public bool Parallel { get; set; }

        public bool WarnDowngrade { get; set; }

        public bool DisallowDowngrade { get; set; }
    }
}