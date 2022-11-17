namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

using System.Collections.Generic;

public class RepositoryActionForEachV1 : RepositoryAction
{
    public List<RepositoryAction> Actions { get; set; } = new List<RepositoryAction>();

    public string? Variable { get; set; }

    public string? Enumerable { get; set; }

    public string? Skip { get; set; }
}