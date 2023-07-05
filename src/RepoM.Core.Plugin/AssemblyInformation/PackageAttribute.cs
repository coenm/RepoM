namespace RepoM.Core.Plugin.AssemblyInformation;

using System;

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
