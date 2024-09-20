namespace RepoM.Core.Plugin.AssemblyInformation;

using System;

/// <summary>
/// This attribute is required for Plugins.
/// RepoM uses this attribute to extract plugin information without loading the assembly.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class PackageAttribute : Attribute
{
    public PackageAttribute(string name, string toolTip, string description)
    {
        Name = name;
        ToolTip = toolTip;
        Description = description;
    }

    public string Name { get; }

    public string ToolTip { get; }

    public string Description { get; }
}