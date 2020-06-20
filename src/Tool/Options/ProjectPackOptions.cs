namespace PackProject.Tool.Options
{
    public class ProjectPackOptions : PackOptions
    {
        public bool IsDebug { get; set; }

        public string OutputGraph { get; set; }

        public bool Parallel { get; set; }
    }
}