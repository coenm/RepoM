namespace RepoM.Api.Git;

using System.Collections.ObjectModel;

public interface IRepositoryInformationAggregator
{
    void Add(Repository repository, IRepositoryMonitor repositoryMonitor);

    void RemoveByPath(string path);

    string? GetStatusByPath(string path);

    ObservableCollection<RepositoryView> Repositories { get; }

    void Reset();

    bool HasRepository(string path);
}