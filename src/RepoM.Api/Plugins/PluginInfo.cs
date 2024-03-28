namespace RepoM.Api.Plugins;

using System;
using System.IO;
using RepoM.Core.Plugin.AssemblyInformation;

public readonly struct PluginInfo
{
    public PluginInfo(string assemblyPath, PackageAttribute? packageAttribute, byte[] hash)
    {
        AssemblyPath = assemblyPath ?? throw new ArgumentNullException(nameof(assemblyPath));
        Hash = hash ?? throw new ArgumentNullException(nameof(hash));

        if (packageAttribute != null)
        {
            Name = packageAttribute.Name;
            ToolTip = packageAttribute.ToolTip;
            Description = packageAttribute.Description;
        }
        else
        {
            Name = new FileInfo(assemblyPath).Name; // can throw
            ToolTip = string.Empty;
            Description = string.Empty;
        }
    }

    public string AssemblyPath { get; }

    public string Name { get; }

    public string ToolTip { get; }

    public string Description { get; }

    public byte[] Hash { get; }
}