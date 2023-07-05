namespace RepoM.Api.Plugins;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using RepoM.Core.Plugin.AssemblyInformation;

public class PluginFinder : IPluginFinder
{
    private readonly IFileSystem _fileSystem;
    private readonly IHmacService _hmacService;

    public PluginFinder(IFileSystem fileSystem, IHmacService hmacService)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _hmacService = hmacService ?? throw new ArgumentNullException(nameof(hmacService));
    }

    public IEnumerable<PluginInfo> FindPlugins(string baseDirectory)
    {
        return FindPluginAssemblies(baseDirectory)
          .Select(assemblyPath => GetPluginInfo(assemblyPath.FullName));
    }

    private IEnumerable<IFileInfo> FindPluginAssemblies(string baseDirectory)
    {
        IEnumerable<IFileInfo> assemblies = FindPluginFileNames(baseDirectory);

        foreach (var dir in GetPluginDirectories(baseDirectory))
        {
            assemblies = assemblies.Concat(FindPluginFileNames(dir));
        }

        return assemblies;
    }

    private IEnumerable<string> GetPluginDirectories(string baseDirectory)
    {
        var pluginBaseDirectory = Path.Combine(baseDirectory, "Plugins");

        if (!_fileSystem.Directory.Exists(pluginBaseDirectory))
        {
            return Enumerable.Empty<string>();
        }

        return _fileSystem.Directory.EnumerateDirectories(pluginBaseDirectory);
    }

    private IEnumerable<IFileInfo> FindPluginFileNames(string baseDirectory)
    {
        return _fileSystem.DirectoryInfo.New(baseDirectory)
          .GetFiles()
          .Where(file =>
             file.Name.StartsWith("RepoM.Plugin.")
             &&
             file.Extension.ToLower() == ".dll");
    }
    
    private PluginInfo GetPluginInfo(string assemblyPath)
    {
        using FileSystemStream stream = _fileSystem.File.OpenRead(assemblyPath);

        stream.Position = 0;
        byte[] hash = _hmacService.GetHmac(stream);

        stream.Position = 0;
        PackageAttribute? packageAttribute = PackageAttributeReader.ReadPackageDataWithoutLoadingAssembly(stream);

        return new PluginInfo(assemblyPath, packageAttribute, hash);
    }
}