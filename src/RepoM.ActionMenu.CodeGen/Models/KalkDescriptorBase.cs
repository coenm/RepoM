namespace RepoM.ActionMenu.CodeGen.Models;

using System.Collections.Generic;
using RepoM.ActionMenu.CodeGen.Models.New;

public abstract class KalkDescriptorBase : IXmlDocsExtended
{
    public List<string> Names { get; } = new();

    public bool IsCommand { get; set; }

    public string Category { get; set; }

    public string Description { get; set; }

    public List<ParamDescriptor> Params { get; } = new();

    public string Syntax { get; set; }

    public string Returns { get; set; }

    public string Remarks { get; set; }

    public ExamplesDescriptor? Examples { get; set; }
}