namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

using System;
using System.Collections.Generic;

[Obsolete("Old action menu")]
public class RepositoryActionConfiguration
{
    public Redirect? Redirect { get; set; }

    public List<FileReference> RepositorySpecificEnvironmentFiles { get; set; } = new List<FileReference>();

    public List<FileReference> RepositorySpecificConfigFiles { get; set; } = new List<FileReference>();

    public List<Variable> Variables { get; set; } = new List<Variable>();

    public TagsCollection TagsCollection { get; set; } = new TagsCollection();

    public ActionsCollection ActionsCollection { get; set; } = new ActionsCollection();
}