using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PackProject.Tool.Options;
using PackProject.Tool.Services.Runner;

namespace PackProject.Tool.Services.DotNetPack
{
    public class DotNetPack : IDotNetPack
    {
        private readonly Lazy<PropertyHolder[]> _propertiesCache;

        private readonly ICommandRunner _commandRunner;
        private readonly ILogger<DotNetPack> _logger;

        public DotNetPack(ICommandRunner commandRunner, ILogger<DotNetPack> logger)
        {
            _commandRunner = commandRunner;
            _logger = logger;
            _propertiesCache = new Lazy<PropertyHolder[]>(() =>
            {
                return typeof(PackOptions).GetTypeInfo().DeclaredProperties
                    .Where(p => p.CanRead && p.CanWrite)
                    .Select(p => new PropertyHolder
                    {
                        Property = p,
                        Option = p.GetCustomAttribute<DotnetOptionAttribute>()
                    })
                    .Where(s => s.Option != null)
                    .ToArray();
            });
        }

        /// <inheritdoc />
        public Task PackAsync(string projectPath, PackOptions options)
        {
            var args = CreateArguments(projectPath, options);
            return _commandRunner.RunAsync("dotnet", "pack", args);
        }

        public string[] CreateArguments(string projectPath, PackOptions options)
        {
            var arguments = new List<string> { projectPath };
            var properties = _propertiesCache.Value;

            foreach (var property in properties)
            {
                var propertyType = property.Property.PropertyType;
                var propertyValue = property.Property.GetValue(options);

                if (propertyValue == null)
                {
                    _logger.LogDebug("Skipping {Property} with alias {Alias} because it's value is null",
                        property.Property.Name, property.Option.Alias);

                    continue;
                }

                if (propertyType == typeof(bool))
                {
                    if ((bool)propertyValue)
                    {
                        arguments.Add(property.Option.Alias);

                        _logger.LogDebug("Added flag {Property} with alias {Alias}",
                            property.Property.Name, property.Option.Alias);
                    }
                    else
                    {
                        _logger.LogDebug("Skipping {Property} with alias {Alias} because it's value is false",
                            property.Property.Name, property.Option.Alias);
                    }
                }
                else
                {
                    var converter = TypeDescriptor.GetConverter(propertyType);
                    var strValue = converter.ConvertToInvariantString(propertyValue);

                    arguments.Add(property.Option.Alias);
                    arguments.Add(strValue);

                    _logger.LogDebug("Added property {Property} with alias {Alias} and value {Value}",
                        property.Property.Name, property.Option.Alias, strValue);
                }
            }

            if (options.Parameters != null)
            {
                foreach (var parameter in options.Parameters)
                {
                    arguments.Add(parameter);
                    _logger.LogDebug("Added {Parameter}", parameter);
                }
            }

            return arguments.ToArray();
        }

        private class PropertyHolder
        {
            public PropertyInfo Property { get; set; }

            public DotnetOptionAttribute Option { get; set; }
        }
    }
}