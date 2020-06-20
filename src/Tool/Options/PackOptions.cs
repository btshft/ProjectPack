using System;

namespace PackProject.Tool.Options
{
    public class PackOptions
    {
        public string[] Parameters { get; set; } = Array.Empty<string>();

        public string ProjectPath { get; set; }

        [DotnetOption("--configuration")]
        public string Configuration { get; set; }

        [DotnetOption("--force")]
        public bool Force { get; set; }

        [DotnetOption("--include-source")]
        public bool IncludeSource { get; set; }

        [DotnetOption("--interactive")]
        public bool Interactive { get; set; }

        [DotnetOption("--no-build")]
        public bool NoBuild { get; set; }

        [DotnetOption("--no-restore")]
        public bool NoRestore { get; set; }

        [DotnetOption("--no-logo")]
        public bool NoLogo { get; set; }

        [DotnetOption("--output")]
        public string Output { get; set; }

        [DotnetOption("--runtime")]
        public string Runtime { get; set; }

        [DotnetOption("--serviceable")]
        public bool Serviceable { get; set; }

        [DotnetOption("--verbosity")]
        public string Verbosity { get; set; }

        [DotnetOption("--version-suffix")]
        public string VersionSuffix { get; set; }
    }
}