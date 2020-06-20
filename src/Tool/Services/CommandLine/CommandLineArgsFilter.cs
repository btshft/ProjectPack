using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PackProject.Tool.Services.CommandLine
{
    public class CommandLineArgsFilter : ICommandLineArgsFilter
    {
        /// <summary>
        /// -p:Param=1234
        /// /p:Param=235
        /// </summary>
        private static readonly Regex ParameterRegex = new Regex(
            @"^[\/-]p:(?:.)+$", RegexOptions.Compiled | RegexOptions.IgnoreCase,
            TimeSpan.FromSeconds(15));

        /// <summary>
        /// --flag value
        /// --flag
        /// -f value
        /// </summary>
        private static readonly Regex OptionRegex = new Regex(
            @"^-(?:[-])*.*$", RegexOptions.Compiled | RegexOptions.IgnoreCase,
            TimeSpan.FromSeconds(15));

        private static readonly string[] Commands = new[]
        {
            "pack-project",         // run as dotnet project-pack (projectPath) ..
            "dotnet-pack-project",  // run as dotnet tool run dotnet-pack-project (projectPath) ..
        };

        private static readonly string[] Introduced = new []
        {
            "--debug",
            "--parallel",
            "--project", 
        };

        private readonly string[] _args;

        public CommandLineArgsFilter(string[] args)
        {
            _args = args;
        }

        /// <inheritdoc />
        public string GetProjectPath()
        {
            if (_args.Length < 1)
                return null;

            foreach (var command in Commands)
            {
                var index = Array.IndexOf(_args, command);
                if (index < 0)
                    continue;

                // We're here now:
                // dotnet pack-project
                //        ^^^^^^^^^^^^ (idx: 0)

                // or here:
                // dotnet tool run dotnet-pack-project
                //                 ^^^^^^^^^^^^^^^^^^^ (idx: 2)

                if (index < _args.Length + 1)
                {
                    var nextToken = _args[index + 1];

                    // No path was provided, so format is like:
                    // dotnet pack-project --output ./artifacts
                    if (OptionRegex.IsMatch(nextToken))
                        return null;

                    return nextToken;
                }
            }

            // dotnet run PackProject.Tool.dll ./project --output ./output
            //            ^^^^^^^^^^^^^^^^^^^^ (idx: 1)

            var libPosition = GetLibraryPosition(_args);
            if (libPosition.HasValue)
            {
                var hasNextToken = (libPosition.Value + 1) < _args.Length;

                // dotnet run Lib.dll
                if (!hasNextToken)
                    return null;

                var nextToken = _args[libPosition.Value + 1];
                if (OptionRegex.IsMatch(nextToken) || ParameterRegex.IsMatch(nextToken))
                    return null;

                return nextToken;
            }

            // Probably only for debug mode
            return _args[0];
        }

        /// <inheritdoc />
        public string[] GetAll()
        {
            return _args.ToArray();
        }

        /// <inheritdoc />
        public string[] GetBypass()
        {
            static bool IsOriginal(string arg)
            {
                return !Introduced.Any(i => string
                    .Equals(i, arg, StringComparison.InvariantCultureIgnoreCase));
            }

            if (_args.Length < 1)
                return _args.ToArray();

            var filteredArgs = _args.Where(IsOriginal).ToList();
            foreach (var command in Commands)
            {
                if (filteredArgs.Contains(command))
                    filteredArgs.Remove(command);
            }

            // Trim dll name when debugging
            // dotnet run PackProject.Tool.dll (...)
            var libPosition = GetLibraryPosition(filteredArgs);
            if (libPosition.HasValue)
                filteredArgs.RemoveAt(libPosition.Value);

            return filteredArgs.ToArray();
        }

        /// <inheritdoc />
        public string[] GetParameters()
        {
            var result = new List<string>();
            foreach (var arg in _args)
            {
                if (ParameterRegex.IsMatch(arg))
                    result.Add(arg);
            }

            return result.ToArray();
        }

        private static int? GetLibraryPosition(IReadOnlyList<string> args)
        {
            for(var i = 0; i < args.Count; i++)
            {
                var isDll = args[i].EndsWith("PackProject.Tool.dll", StringComparison.InvariantCultureIgnoreCase);
                if (isDll)
                {
                    return i;
                }
            }
            return null;
        }
    }
}