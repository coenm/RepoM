namespace RepoM.ActionMenu.CodeGen.Models;

using System.Collections.Generic;
using RepoM.ActionMenu.CodeGen.Models.New;

public class KalkModuleToGenerate : KalkDescriptorToGenerate
{
    public KalkModuleToGenerate()
    {
        Members = new List<KalkMemberToGenerate>();
        IsModule = true;
    }

    public string Name { get; set; }

    public string Title { get; set; }
    
    public string Namespace { get; set; }

    public string ClassName { get; set; }

    public List<KalkMemberToGenerate> Members { get; }

    public ActionMenuContextClassDescriptor NewStyle { get; set; }
}

public class ActionsToGenerate : KalkDescriptorToGenerate
{
    public ActionsToGenerate()
    {
        Properties = new List<ActionPropertyToGenerate>();
        IsModule = true;
    }

    public string Name { get; set; }

    public string Title { get; set; }

    public string Url { get; set; }

    public string Namespace { get; set; }

    public string ClassName { get; set; }

    public List<ActionPropertyToGenerate> Properties { get; }
}