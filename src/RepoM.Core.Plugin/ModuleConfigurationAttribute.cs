namespace RepoM.Core.Plugin;

using System;

/// <summary>
/// This attribute is optional for Plugins.
/// RepoM uses this attribute to extract module configuration objects for documentation purposes.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ModuleConfigurationAttribute : Attribute
{
    public ModuleConfigurationAttribute(int version)
    {
        Version = version;
    }

    public int Version { get; }
}