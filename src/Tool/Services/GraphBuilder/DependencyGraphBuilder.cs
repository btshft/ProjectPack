using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PackProject.Tool.Extensions;
using PackProject.Tool.Helpers;
using PackProject.Tool.Models;
using PackProject.Tool.Services.CommandLine;
using PackProject.Tool.Services.ExecutionContext;
using PackProject.Tool.Services.Runner;

namespace PackProject.Tool.Services.GraphBuilder
{
    public class DependencyGraphBuilder : IDependencyGraphBuilder
    {
        private readonly ILogger<DependencyGraphBuilder> _logger;
        private readonly IExecutionContextAccessor _contextAccessor;
        private readonly ICommandRunner _commandRunner;
        private readonly ICommandLineArgsFilter _commandLineArgsFilter;

        public DependencyGraphBuilder(
            ILogger<DependencyGraphBuilder> logger, 
            IExecutionContextAccessor contextAccessor, 
            ICommandRunner commandRunner, 
            ICommandLineArgsFilter commandLineArgsFilter)
        {
            _logger = logger;
            _contextAccessor = contextAccessor;
            _commandRunner = commandRunner;
            _commandLineArgsFilter = commandLineArgsFilter;
        }

        /// <inheritdoc />
        public async Task<DependencyGraph> BuildAsync()
        {
            var context = _contextAccessor.Context;

            async Task<DependencyGraphFile> GenerateAsync()
            {
                var graphFile = DependencyGraphFile.Create();
                var arguments = new[]
                {
                    context.ProjectPath,
                    MsBuild.Property("Configuration", context.Configuration),
                    MsBuild.Target("Restore", "GenerateRestoreGraphFile"),
                    MsBuild.Property("RestoreGraphOutputPath", graphFile.Path),
                };

                var parameters = _commandLineArgsFilter.GetParameters();
                await _commandRunner.RunAsync("dotnet", "msbuild", arguments.Extend(parameters));

                if (context.IsDebugMode)
                    _logger.LogDebug($"Graph file: {graphFile}");

                return graphFile;
            }

            async Task<DependencyGraph> ParseAsync(string graphPath)
            {
                await using var fileStream = File.OpenRead(graphPath);

                var model = await JsonSerializer.DeserializeAsync<DependencyGraph>(fileStream, new JsonSerializerOptions
                {
                    IgnoreNullValues = true,
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                });

                if (context.IsDebugMode)
                    _logger.LogDebug($"Graph:{Environment.NewLine} {model}");

                return model;
            }

            using var file = await GenerateAsync();
            return await ParseAsync(file.Path);
        }
    }
}