namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;

using System.Collections.Generic;

public interface IRepositoryTagsFactory
{
    IEnumerable<string> GetTags(RepoZ.Api.Git.Repository repository);
}