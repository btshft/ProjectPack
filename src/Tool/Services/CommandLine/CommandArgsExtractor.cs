using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace PackProject.Tool.Services.CommandLine
{
    public class CommandArgsExtractor : ICommandArgsExtractor
    {
        /// <summary>
        /// -p:Param=1234
        /// /p:Param=235
        /// </summary>
        private static readonly Regex ParameterRegex = new Regex(
            @"^[\/-]p:(?:.)+$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
            TimeSpan.FromSeconds(15));

        /// <summary>
        /// --flag value
        /// --flag
        /// -f value
        /// </summary>
        private static readonly Regex OptionRegex = new Regex(
            @"^-(?:[-])*.*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
            TimeSpan.FromSeconds(15));

        private static readonly Regex VerbosityOptionRegex = new Regex(
            @"^(?:-v)|(?:--verbosity)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
            TimeSpan.FromSeconds(15));

        private static readonly Regex VerbosityLevelRegex = new Regex(
            @"^(?:(?:q|quiet)|(?:m|minimal)|(?:n|normal)|(?:d|detailed))$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
            TimeSpan.FromSeconds(15));

        private static readonly string[] Commands = new[]
        {
            "pack-project",         // run as dotnet project-pack (projectPath) ..
            "dotnet-pack-project",  // run as dotnet tool run dotnet-pack-project (projectPath) ..
        };

        private readonly string[] _args;

        public CommandArgsExtractor(string[] args)
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

                // Check if we still can move next so command
                // has at least 1 argument after name
                if ((index + 1) < _args.Length)
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

            return OptionRegex.IsMatch(_args[0])
                ? null : _args[0];
        }

        /// <inheritdoc />
        public string[] GetParameters()
        {
            return _args.Where(arg => ParameterRegex.IsMatch(arg)).ToArray();
        }

        /// <inheritdoc />
        public string[] GetAll()
        {
            return _args.ToArray();
        }

        /// <inheritdoc />
        public string GetVerbosity()
        {
            for (var i = 0; i < _args.Length; i++)
            {
                var token = _args[i];
                var nextToken = (i + 1) < _args.Length
                    ? _args[i + 1]
                    : null;

                if (VerbosityOptionRegex.IsMatch(token) && nextToken != null && VerbosityLevelRegex.IsMatch(nextToken))
                    return nextToken;
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int? GetLibraryPosition(IReadOnlyList<string> args)
        {
            for (var i = 0; i < args.Count; i++)
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