namespace RepoM.ActionMenu.CodeGen.Models;

using System.Collections.Generic;

public interface IXmlDocsExtended
{
    string Description { get; set; }

    string Returns { get; set; }

    string Remarks { get; set; }

    ExamplesDescriptor? Examples { get; set; }

    List<ParamDescriptor> Params { get; } 
}

public abstract class KalkDescriptor : IXmlDocsExtended
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