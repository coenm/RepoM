namespace RepoM.ActionMenu.CodeGen.Models;

using System.Collections.Generic;
using System.Diagnostics;

[DebuggerDisplay($"{{{nameof(ClassName)},nq}}")]
public class ActionMenuClassDescriptor : ClassDescriptor
{
    /// <summary>
    /// Properties
    /// </summary>
    public List<ActionMenuMemberDescriptor> ActionMenuProperties { get; } = [];

    public string RepositoryActionName => Name;

    public override void Accept(IClassDescriptorVisitor visitor)
    {
        visitor.Visit(this);
    }
}

[DebuggerDisplay($"{{{nameof(ClassName)},nq}}")]
public class ModuleConfigurationClassDescriptor : ClassDescriptor
{
    public string? DefaultValueJson { get; set; }

    public List<PluginConfigurationMemberDescriptor> Properties { get; } = [];
    
    public bool IsObsolete { get; set; }

    public override void Accept(IClassDescriptorVisitor visitor)
    {
        visitor.Visit(this);
    }
}