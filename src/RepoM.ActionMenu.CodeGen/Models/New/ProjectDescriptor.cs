namespace RepoM.ActionMenu.CodeGen.Models.New;

using System.Collections.Generic;
using RepoM.Core.Plugin.AssemblyInformation;

public class PluginProjectDescriptor : ProjectDescriptor
{
    public PackageAttribute PackageAttribute { get; set; } = null!;
}

public class ProjectDescriptor
{
    public string ProjectName { get; set; } = null!;

    public List<ActionMenuClassDescriptor> ActionMenus { get; } = new List<ActionMenuClassDescriptor>();

    public List<ActionMenuContextClassDescriptor> ActionContextMenus { get; } = new List<ActionMenuContextClassDescriptor>();

    public List<ClassDescriptor> Types { get; } = new List<ClassDescriptor>();
}

public class ActionMenuContextClassDescriptor : ClassDescriptor
{
}

public class ActionMenuClassDescriptor : ClassDescriptor
{

}

public class ClassDescriptor
{

}