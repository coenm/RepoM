namespace RepoM.App.Plugins;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Mono.Cecil;
using RepoM.Core.Plugin.AssemblyInformation;
using System.Security.Cryptography;

internal class PluginFinder : IPluginFinder
{
    private readonly IFileSystem _fileSystem;

    public PluginFinder(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public IEnumerable<PluginInfo> FindPlugins(string baseDirectory)
    {
        foreach (IFileInfo assemblyPath in FindPluginAssemblies(baseDirectory))
        {
            yield return GetPluginInfo(assemblyPath.FullName);
        }
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
        byte[] hash;
        using FileSystemStream stream = _fileSystem.File.OpenRead(assemblyPath);

        byte[] key = RandomNumberGenerator.GetBytes(64);

        using (var hmac = new HMACSHA256(key))
        {
            hash = key.Concat(hmac.ComputeHash(stream)).ToArray(); // :-)
        }
        stream.Position = 0;

        PackageAttribute? packageAttribute = ReadPackageDataWithoutLoadingAssembly(stream);
        return new PluginInfo(assemblyPath, packageAttribute, hash);
    }

    private static PackageAttribute? ReadPackageDataWithoutLoadingAssembly(Stream stream)
    {
        using var assembly = AssemblyDefinition.ReadAssembly(stream);

        CustomAttribute? attribute = assembly.CustomAttributes.SingleOrDefault(a => a.AttributeType.FullName == typeof(PackageAttribute).FullName);

        if (attribute == null)
        {
            return null;
        }

        IList<CustomAttributeArgument> constructorArguments = attribute.ConstructorArguments;

        if (constructorArguments.Count != 2)
        {
            throw new Exception("Invalid number of constructor arguments");
        }

        var key = constructorArguments[0].Value.ToString();
        var value = constructorArguments[1].Value.ToString();
        return new PackageAttribute(key!, value!);
    }
}