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
    public required string AssemblyName { get; init; }

    /// <summary>
    /// Project name
    /// </summary>
    public required string ProjectName { get; init; }

    /// <summary>
    /// Full filename of the sln or csproj.
    /// </summary>
    public required string FullFilename { get; init; }

    /// <summary>
    /// The directory of the project.
    /// </summary>
    public required string Directory { get; init; }

    /// <summary>
    /// List of class descriptors for repository actions.
    /// </summary>
    public List<ActionMenuClassDescriptor> ActionMenus { get; } = [];

    /// <summary>
    /// List of class descriptors for context (ie scriban methods, properties)
    /// </summary>
    public List<ActionMenuContextClassDescriptor> ActionContextMenus { get; } = [];

    /// <summary>
    /// Regular types (to be used when action type has subtype property)
    /// </summary>
    public List<ClassDescriptor> Types { get; } = [];

    /// <summary>
    /// when project is plugin, the pluginname.
    /// </summary>
    public string? PluginName { get; private set; }

    /// <summary>
    /// when project is plugin, the plugin description.
    /// </summary>
    public string? PluginDescription { get; private set; }

    /// <summary>
    /// when project is plugin, the plugin markdown description.
    /// </summary>
    public string? PluginMarkdownDescription { get; private set; }

    /// <summary>
    /// is plugin or not.
    /// </summary>
    public bool IsPlugin { get; private set; }
    
    [ScriptMemberIgnore]
    public void SetPackageInformation(PackageAttribute attribute)
    {
        PluginName = attribute.Name;
        PluginDescription = attribute.ToolTip;
        PluginMarkdownDescription = attribute.Description;
        IsPlugin = true;
    }
}