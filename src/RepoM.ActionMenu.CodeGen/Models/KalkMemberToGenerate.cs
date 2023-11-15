namespace RepoM.ActionMenu.CodeGen.Models;

public class KalkMemberToGenerate : KalkDescriptorToGenerate
{
    public string Name { get; set; }

    public string XmlId { get; set; }

    public string CSharpName { get; set; }

    public bool IsFunc { get; set; }

    public bool IsAction { get; set; }

    public bool IsConst { get; set; }

    public string Cast { get; set; }

    //public KalkModuleToGenerate Module { get; set; }
}

public class ActionPropertyToGenerate : KalkDescriptorToGenerate /* todo other inheritance?! */
{
    /// <summary>
    ///  Friendly name (for instance, by attribute)
    /// </summary>
    public string Name { get; init; }

    public string XmlId { get; init; }

    /// <summary>
    /// C# member name.
    /// </summary>
    public string CSharpName { get; set; }

    public bool IsConst { get; set; }

    public KalkModuleToGenerate Module { get; set; }
}