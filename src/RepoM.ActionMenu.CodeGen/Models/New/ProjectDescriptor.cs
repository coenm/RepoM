namespace RepoM.ActionMenu.CodeGen.Models.New;

using System.Collections.Generic;

public class ProjectDescriptor
{
    public string AssemblyName { get; set; } = null!;

    public string ProjectName { get; set; } = null!;

    public List<ActionMenuClassDescriptor> ActionMenus { get; } = new List<ActionMenuClassDescriptor>();

    public List<ActionMenuContextClassDescriptor> ActionContextMenus { get; } = new List<ActionMenuContextClassDescriptor>();

    public List<ClassDescriptor> Types { get; } = new List<ClassDescriptor>();
}