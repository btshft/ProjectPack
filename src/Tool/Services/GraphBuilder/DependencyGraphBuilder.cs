using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PackProject.Tool.Extensions;
using PackProject.Tool.Helpers;
using PackProject.Tool.Models;
using PackProject.Tool.Services.ExecutionContext;
using PackProject.Tool.Services.Runner;

namespace PackProject.Tool.Services.GraphBuilder
{
    public class DependencyGraphBuilder : IDependencyGraphBuilder
    {
        private readonly ILogger<DependencyGraphBuilder> _logger;
        private readonly IExecutionContextAccessor _contextAccessor;
        private readonly ICommandRunner _commandRunner;

        public DependencyGraphBuilder(
            ILogger<DependencyGraphBuilder> logger, 
            IExecutionContextAccessor contextAccessor, 
            ICommandRunner commandRunner)
        {
            _logger = logger;
            _contextAccessor = contextAccessor;
            _commandRunner = commandRunner;
        }

        /// <inheritdoc />
        public async Task<DependencyGraph> BuildAsync()
        {
            var options = _contextAccessor.Context.Options;

            async Task<DependencyGraphFile> GenerateAsync()
            {
                var graphFile = string.IsNullOrEmpty(options.OutputGraph)
                    ? DependencyGraphFile.Create()
                    : DependencyGraphFile.Create(options.OutputGraph, autoDelete: false);

                var arguments = new[]
                {
                    options.ProjectPath,
                    MsBuild.Property("Configuration", options.Configuration),
                    MsBuild.Target("Restore", "GenerateRestoreGraphFile"),
                    MsBuild.Property("RestoreGraphOutputPath", graphFile.FilePath),
                };

                await _commandRunner.RunAsync("dotnet", "msbuild", arguments.Extend(options.Parameters));

                if (options.IsDebug)
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

                if (options.IsDebug)
                    _logger.LogDebug($"Graph:{Environment.NewLine} {model}");

                return model;
            }

            using var file = await GenerateAsync();
            return await ParseAsync(file.FilePath);
        }
    }
}