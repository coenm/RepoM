namespace RepoM.Core.Plugin.AssemblyInformation;

using System;

/// <summary>
/// This attribute is required for Plugins.
/// RepoM uses this attribute to extract plugin information without loading the assembly.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class PackageAttribute : Attribute
{
    public PackageAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public string Name { get; set; }

    public string Description { get; set; }
}
