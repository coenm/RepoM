namespace RepoM.Plugin.Demo;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Reflection;
using RepoM.Api.Plugins;

/// <summary>Selects only this startup plugin's application package, excluding external plugins.</summary>
public class DemoPluginFinderDecorator : IPluginFinder
{
    private readonly IFileSystem _fileSystem;
    private readonly IHmacService _hmacService;

    public DemoPluginFinderDecorator(IPluginFinder decoratee, IFileSystem fileSystem, IHmacService hmacService)
    {
        ArgumentNullException.ThrowIfNull(decoratee);
        _fileSystem = fileSystem;
        _hmacService = hmacService;
    }

    protected virtual Assembly PackageAssembly => typeof(DemoPackage).Assembly;

    public IEnumerable<PluginInfo> FindPlugins(string baseDirectory)
    {
        using var stream = _fileSystem.File.OpenRead(PackageAssembly.Location);
        return [new PluginInfo(PackageAssembly.Location, null, _hmacService.GetHmac(stream), isRequired: true)];
    }
}
