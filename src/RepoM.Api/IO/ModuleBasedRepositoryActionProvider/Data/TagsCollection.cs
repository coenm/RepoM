namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

using System.Collections.Generic;

public class TagsCollection
{
    public List<Variable> Variables { get; set; } = new();

    public List<RepositoryActionTag> Tags { get; set; } = new();
}