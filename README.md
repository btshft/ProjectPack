# Project Pack ![PackProject NuGet](https://img.shields.io/nuget/vpre/btshft.pack-project)

Project packer is a simple dotnet tool that allows you to package projects with 'project-to-project' dependencies. 
Tool execution result is the original project packed in NuGet and all the project dependency graph packaged separate NuGets with the same version as the original project.
Basically, the tool is just a wrapper over `dotnet msbuild` and `dotnet pack`.

# Installation
Tool can be installed from [NuGet]() as
```
dotnet tool install --global btshft.pack-project --version 1.0.0-preview.1
```

# Usage

The program call is the same as the [dotnet pack](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-pack), except for a few extra arguments. 

```
dotnet pack-project <project> <args>
```

## Extra arguments
* `--parallel` - Enables parallel package creation.
* `--debug` - Enables display of tool execution flow.

# Why
There are [some inconvenience](https://github.com/dotnet/sdk/issues/6688) with the way `dotnet pack` works with project-to-project references. 

If you simply call `dotnet pack` on a project with such dependencies, 
it will create single project package with 'ProjectReference' replaced by 'PackageReference' with the provided version. 
So you need to publish packages for the entire project dependency graph with the same version as you specified for the current project. **It's a mess.** 

There are some ways to solve 'inconvenience' like [setting PrivateAssets="all"](https://www.jacobmohl.dk/til/use-project-to-project-references-in-nuget/) or [hacking csproj](https://github.com/dotnet/sdk/issues/6688#issuecomment-333318028) which can lead to runtime exceptions.

Therefore, this tool was created trying to cope with project-to-project dependencies in NuGet packages and make working with them not so terrible.
