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


/// <summary>
/// This attribute is optional for Plugins.
/// RepoM uses this attribute to extract module configuration objects for documentation purposes.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ModuleConfigurationAttribute : Attribute
{
    public string DefaultValueFactoryMethod { get; }

    public ModuleConfigurationAttribute(string defaultValueFactoryMethod)
    {
        DefaultValueFactoryMethod = defaultValueFactoryMethod;
    }

    public ModuleConfigurationAttribute()
    {
        
    }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class ModuleConfigurationDefaultValueFactoryMethodAttribute : Attribute
{
    public ModuleConfigurationDefaultValueFactoryMethodAttribute()
    {

    }
}
