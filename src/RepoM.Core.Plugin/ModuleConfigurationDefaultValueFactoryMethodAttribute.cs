namespace RepoM.Core.Plugin;

using System;

/// <summary>
/// This attribute is optional for Plugins.
/// RepoM uses this attribute to create the default configuration for documentation purposes.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ModuleConfigurationDefaultValueFactoryMethodAttribute : Attribute
{
}