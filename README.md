# Project Pack [![PackProject NuGet](https://img.shields.io/nuget/vpre/btshft.pack-project)](https://www.nuget.org/packages/btshft.pack-project/)

> **⚠️ Preview**
> 
> Tool is in preview stage and may not work as expected. In case of errors, feel free to create an issue.

Project packer is a simple dotnet tool that allows you to package projects with 'project-to-project' dependencies. 
Tool execution result is the original project packed in NuGet and all the project dependency graph packaged separate NuGets with the same version as the original project.
Basically, the tool is just a wrapper over `dotnet msbuild` and `dotnet pack`. 

Also Project Pack supports package downgrade check - it scans remote NuGet sources for bundled packages and compares remote versions to locals. If remote contains higher versions, the tool will fail or warn about downgrade depending on provided options. Downgrade scan is disabled by default.

# Installation
Tool can be installed from [NuGet](https://www.nuget.org/packages/btshft.pack-project/) as
```
dotnet tool install --global btshft.pack-project --version 1.0.0-preview.4
```

# Usage

The program call is the same as the [dotnet pack](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-pack), except for a few extra cases descibed below. 

```
dotnet pack-project <project-path> <args>
```

## Examples

Following example will pack project 'MyProject' and all of dependencies to folder './artifacts' with version '1.1.0'.
```
dotnet pack-project ./src/MyProject --output ./artifacts -p:Version=1.1.0
```

## Extra arguments
* `--parallel` - Enables parallel package creation.
* `--output-graph` - Generates project dependency graph in path specified.
* `--warn-downgrade` - Warns if package downgrade detected for any of bundling packages.
* `--disallow-downgrade` - Terminates execution with non-zero code if package downgrade detected.

## Unsupported arguments
* `--no-dependencies` - it's mutual exclusive with tool purpose.

# Why
There are [some inconvenience](https://github.com/dotnet/sdk/issues/6688) that are still [not resolved](https://github.com/NuGet/Home/issues/3891) with the way `dotnet pack` works with project-to-project references. 

If you simply call `dotnet pack` on a project with such dependencies, 
it will create single project package with 'ProjectReference' replaced by 'PackageReference' with the provided version. 
So you need to publish packages for the entire project dependency graph with the same version as you specified for the current project. **It's a mess.** 

There are some ways to solve 'inconvenience' like [setting PrivateAssets="all"](https://www.jacobmohl.dk/til/use-project-to-project-references-in-nuget/) or [hacking csproj](https://github.com/dotnet/sdk/issues/6688#issuecomment-333318028) which can lead to runtime exceptions.

Therefore, this tool was created trying to cope with project-to-project dependencies in NuGet packages and make working with them not so terrible.

# How it works
Tool inspired by article [Analyzing .NET Core project dependencies](https://www.jerriepelser.com/blog/analyze-dotnet-project-dependencies-part-1/). It uses the same mechanism to analyze dependencies and build a graph, through a MSBuild `GenerateRestoreGraphFile` targer after which the tool analyzes dependencies and for each of them translates the call to dotnet pack.
