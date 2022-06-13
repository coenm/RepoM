namespace RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider;

using System.Collections.Generic;
using RepoM.Api.Git;

public interface IRepositoryTagsFactory
{
    IEnumerable<string> GetTags(Repository repository);
}