namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

using System;
using System.Collections.Generic;

[Obsolete("Old action menu ?")]
public class TagsCollection
{
    public List<Variable> Variables { get; set; } = new();

    public List<RepositoryActionTag> Tags { get; set; } = new();
}