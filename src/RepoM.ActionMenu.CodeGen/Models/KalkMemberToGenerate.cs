namespace RepoM.ActionMenu.CodeGen.Models;

public sealed class KalkMemberToGenerate : KalkDescriptorToGenerateBase
{
    public string Name { get; set; }

    public string XmlId { get; set; }

    public string CSharpName { get; set; }

    public bool IsFunc { get; set; }

    public bool IsAction { get; set; }

    public bool IsConst { get; set; }

    public string Cast { get; set; }
}