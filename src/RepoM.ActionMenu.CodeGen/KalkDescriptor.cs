namespace RepoM.ActionMenu.CodeGen;

using System.Collections.Generic;

public class KalkDescriptor
{
    public List<string> Names { get; } = new();

    public bool IsCommand { get; set; }

    public string Category { get; set; }

    public string Description { get; set; }

    public List<KalkParamDescriptor> Params { get; } = new();

    public string Syntax { get; set; }

    public string Returns { get; set; }

    public string Remarks { get; set; }

    public List<ExamplesDescriptor> Examples { get; } = new();
}