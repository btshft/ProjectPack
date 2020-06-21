using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaGet.Protocol;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using PackProject.Tool.Exceptions;
using PackProject.Tool.Extensions;
using PackProject.Tool.Models;
using PackProject.Tool.Services.ExecutionContext;
using PackProject.Tool.Services.GraphAnalyzer;

namespace PackProject.Tool.Services.GraphProcessor
{
    public class PackageDowngradeScanner : IDependencyGraphProcessor
    {
        private readonly ILogger<PackageDowngradeScanner> _logger;
        private readonly IExecutionContextAccessor _contextAccessor;

        public PackageDowngradeScanner(ILogger<PackageDowngradeScanner> logger, IExecutionContextAccessor contextAccessor)
        {
            _logger = logger;
            _contextAccessor = contextAccessor;
        }


        /// <inheritdoc />
        public async Task ProcessAsync(DependencyGraphAnalysis analysis, DependencyGraph graph)
        {
            static bool IsHttp(Uri uri)
            {
                return uri.Scheme == Uri.UriSchemeHttps ||
                       uri.Scheme == Uri.UriSchemeHttp;
            }

            var options = _contextAccessor.Context.Options;

            if (!options.WarnDowngrade && !options.DisallowDowngrade)
                return;

            var downgrades = new List<PackageDowngradeResult>();
            foreach (var source in analysis.PackageSources)
            {
                if (Uri.TryCreate(source, UriKind.Absolute, out var uri) && IsHttp(uri))
                {
                    var results = await ProcessSourceAsync(analysis, source);
                    foreach (var result in results)
                    {
                        _logger.LogWarning(
                            "Detected possible downgrade of package {Package}. {NewLine}" +
                            "Bundling version is '{CurrentVersion}' {NewLine}" +
                            "Remote version is '{HigherVersion}' in source {Source}",
                            result.Project.PackageName, Environment.NewLine, result.Project.Version, Environment.NewLine,
                            result.RemoteVersion.ToNormalizedString(), result.PackageSource);
                    }

                    downgrades.AddRange(results);
                }
                else
                {
                    _logger.LogInformation("Source '{Source}' seems not to be remote source, skipping it", source);
                }
            }

            if (options.DisallowDowngrade)
            {
                var highestDowngrade = downgrades.OrderByDescending(v => v.RemoteVersion)
                    .FirstOrDefault();

                if (highestDowngrade != null)
                {
                    _logger.LogError("Detected {Count} package downgrades.{NewLine}" +
                                     "You should publish new package version higher than '{ExistingVersion}' to satisfy requirements.{NewLine}" +
                                     "Highest package in dependency chain is {PackageName} with version {RemoteVersion} at source {PackageSource}{NewLine}" +
                                     "Attempted to publish version is '{Version}'",
                        downgrades.Count, Environment.NewLine,
                        highestDowngrade.RemoteVersion, Environment.NewLine,
                        highestDowngrade.Project.PackageName, highestDowngrade.RemoteVersion,
                        highestDowngrade.PackageSource, Environment.NewLine,
                        highestDowngrade.Project.Version);

                    throw new NonZeroExitCodeException("Package downgrade", exitCode: 100);
                }
            }
        }

        private async Task<PackageDowngradeResult[]> ProcessSourceAsync(DependencyGraphAnalysis analysis, string source)
        {
            var results = new List<PackageDowngradeResult>();

            try
            {
                var client = new NuGetClient(source);
                foreach (var project in analysis.Projects)
                {
                    var packageVersions = await client.ListPackageVersionsAsync(project.PackageName, default)
                        .WithTimeout(TimeSpan.FromSeconds(30));

                    if (packageVersions == null || packageVersions.Count < 1)
                    {
                        _logger.LogInformation("No versions of package {Package} found at {Source}", project.PackageName, source);
                        continue;
                    }

                    _logger.LogInformation("Found {Count} versions of package {Package} at {Source}: {NewLine} {Packages}",
                        packageVersions.Count, project.PackageName, source, Environment.NewLine,
                        string.Join(Environment.NewLine, packageVersions.Select(v => v.ToNormalizedString())));

                    var newPackageVersion = NuGetVersion.Parse(project.Version);
                    var higherVersion = packageVersions.OrderByDescending(s => s.Version)
                        .FirstOrDefault(v => v > newPackageVersion);

                    if (higherVersion != null)
                    {
                        var projectRef = project;
                        results.Add(new PackageDowngradeResult
                        {
                            Project = projectRef,
                            RemoteVersion = higherVersion,
                            PackageSource = source
                        });
                    }
                }
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("Unable to process packages in source {Source}, reason - Timeout.", source);
            }

            return results.ToArray();
        }

        private class PackageDowngradeResult
        {
            public ProjectDefinition Project { get; set; }

            public NuGetVersion RemoteVersion { get; set; }

            public string PackageSource { get; set; }
        }
    }
}