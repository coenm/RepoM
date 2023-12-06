namespace RepoM.ActionMenu.CodeGen.Models;

using System.Collections.Generic;
using System.Diagnostics;
using RepoM.Core.Plugin.AssemblyInformation;
using Scriban.Runtime;

[DebuggerDisplay("{ProjectName,nq}")]
public sealed class ProjectDescriptor
{
    /// <summary>
    /// Assembly Name
    /// </summary>
    public string AssemblyName { get; set; } = null!;

    /// <summary>
    /// Project name
    /// </summary>
    public string ProjectName { get; set; } = null!;

    /// <summary>
    /// List of class descriptors for repository actions.
    /// </summary>
    public List<ActionMenuClassDescriptor> ActionMenus { get; } = new List<ActionMenuClassDescriptor>();

    /// <summary>
    /// List of class descriptors for context (ie scriban methods, properties)
    /// </summary>
    public List<ActionMenuContextClassDescriptor> ActionContextMenus { get; } = new List<ActionMenuContextClassDescriptor>();

    /// <summary>
    /// Regular types (to be used when action type has sub type property)
    /// </summary>
    public List<ClassDescriptor> Types { get; } = new List<ClassDescriptor>();

    /// <summary>
    /// when project is plugin, the pluginname.
    /// </summary>
    public string? PluginName { get; private set; }

    /// <summary>
    /// when project is plugin, the plugin description.
    /// </summary>
    public string? PluginDescription { get; private set; }

    /// <summary>
    /// is plugin or not.
    /// </summary>
    public bool IsPlugin { get; private set; } = false;

    [ScriptMemberIgnore]
    public void SetPackageInformation(PackageAttribute attribute)
    {
        PluginName = attribute.Name;
        PluginDescription = attribute.Description;
        IsPlugin = true;
    }
}