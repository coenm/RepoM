namespace RepoM.ActionMenu.CodeGen.Models;

using System.Collections.Generic;

public interface IXmlDocsExtended
{
    string Description { get; set; }

    string? InheritDocs { get; set; }

    string Returns { get; set; }

    string Remarks { get; set; }

    ExamplesDescriptor? Examples { get; set; }

    List<ParamDescriptor> Params { get; }
}