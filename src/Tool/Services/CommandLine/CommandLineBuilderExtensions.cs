using System.CommandLine;
using System.CommandLine.Builder;

namespace PackProject.Tool.Services.CommandLine
{
    public static class CommandLineBuilderExtensions
    {
        /// <summary>
        /// https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-pack#arguments
        /// </summary>
        public static CommandLineBuilder AddDotnetPackOptions(this CommandLineBuilder builder)
        {
            builder.AddOption<string>(
                "Defines the build configuration. The default for most projects is Debug, but you can override the build configuration settings in your project.",
                "--configuration", "-c");

            builder.AddOption<bool>(
                "Forces all dependencies to be resolved even if the last restore was successful. Specifying this flag is the same as deleting the project.assets.json file.",
                "--force");

            builder.AddOption<bool>(
                "Includes the debug symbols NuGet packages in addition to the regular NuGet packages in the output directory. The sources files are included in the src folder within the symbols package.",
                "--include-source");

            builder.AddOption<bool>(
                "Includes the debug symbols NuGet packages in addition to the regular NuGet packages in the output directory.",
                "--include-symbols");

            builder.AddOption<bool>(
                "Allows the command to stop and wait for user input or action (for example, to complete authentication).",
                "--interactive");

            builder.AddOption<bool>(
                "Doesn't build the project before packing. It also implicitly sets the --no-restore flag.",
                "--no-build");

            builder.AddOption<bool>(
                "Doesn't execute an implicit restore when running the command.",
                "--no-restore");

            builder.AddOption<bool>(
                "Doesn't display the startup banner or the copyright message.",
                "--nologo");

            builder.AddOption<string>(
                "Places the built packages in the directory specified.",
                "--output", "-o");

            builder.AddOption<string>(
                "Specifies the target runtime to restore packages for.",
                "--runtime");

            builder.AddOption<bool>(
                "Sets the serviceable flag in the package.",
                "--serviceable", "-s");

            builder.AddOption<string>(
                "Defines the value for the $(VersionSuffix) MSBuild property in the project.",
                "--version-suffix");

            builder.AddOption<string>(
                "Sets the verbosity level of the command. Allowed values are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic].",
                "--verbosity", "-v");

            return builder;
        }

        public static CommandLineBuilder AddCustomOptions(this CommandLineBuilder builder)
        {
            builder.AddOption<bool>(
                "Enables parallel package creation.",
                "--parallel");

            builder.AddOption<string>(
                "Set filename for generating project dependency graph.",
                "--output-graph");

            builder.AddOption<bool>(
                "Warn when possible package downgrade detected.",
                "--warn-downgrade");

            builder.AddOption<bool>(
                "Terminate tool execution when package downgrade detected.",
                "--disallow-downgrade");

            return builder;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private static CommandBuilder AddOption<T>(this CommandBuilder builder, string description,
            params string[] aliases)
        {
            return builder.AddOption(new Option<T>(aliases, description));
        }
    }
}