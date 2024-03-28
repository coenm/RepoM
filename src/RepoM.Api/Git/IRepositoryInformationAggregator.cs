namespace RepoM.Api.Git;

using System.Collections.ObjectModel;
using RepoM.Core.Plugin.Repository;

public interface IRepositoryInformationAggregator
{
    void Add(IRepository repository, IRepositoryMonitor repositoryMonitor);

    void RemoveByPath(string path);

    string? GetStatusByPath(string path);

    ObservableCollection<RepositoryViewModel> Repositories { get; }

    void Reset();

    bool HasRepository(string path);
}