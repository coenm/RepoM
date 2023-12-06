namespace RepoM.ActionMenu.CodeGen.Models;

using System.Collections.Generic;

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
}