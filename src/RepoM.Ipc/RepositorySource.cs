namespace RepoM.Ipc;

using System.Linq;
using RepoM.Api.Git;

public class RepositorySource : IRepositorySource
{
    private readonly IRepositoryInformationAggregator _repositoryInformationAggregator;

    public RepositorySource(IRepositoryInformationAggregator repositoryInformationAggregator)
    {
        _repositoryInformationAggregator = repositoryInformationAggregator;
    }

    public Repository[] GetMatchingRepositories(string repositoryNamePattern)
    {
        return _repositoryInformationAggregator
               .Repositories
               .Where(r => r.MatchesRegexFilter(repositoryNamePattern))
               .Select(r => new Repository(r.Name)
                   {
                       BranchWithStatus = r.BranchWithStatus,
                       HasUnpushedChanges = r.HasUnpushedChanges,
                       Path = r.Path,
                   })
               .ToArray();
    }
}