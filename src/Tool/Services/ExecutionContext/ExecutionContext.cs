namespace PackProject.Tool.Services.ExecutionContext
{
    public class ExecutionContext
    {
        public bool IsDebugMode { get; set; }

        public string ProjectPath { get; set; }

        public string Configuration { get; set; }

        public string Output { get; set; }

        public bool RunParallel { get; set; }
    }
}