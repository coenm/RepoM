namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider;

using System.Collections.Generic;
using RepoM.Api.Git;
using RepoM.Core.Plugin.Repository;

public interface IRepositoryTagsFactory
{
    IEnumerable<string> GetTags(IRepository repository);
}