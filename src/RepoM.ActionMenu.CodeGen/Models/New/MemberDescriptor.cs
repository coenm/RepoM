namespace RepoM.ActionMenu.CodeGen.Models.New;

using System;

/// <summary>
/// Property, Function, field etc. etc.
/// </summary>
public class MemberDescriptor
{
    public string CSharpName { get; init; }

    public Type? ReturnType { get; set; }


}