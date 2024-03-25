namespace RepoM.Api.Git;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RepoM.Api.Common;
using RepoM.Core.Plugin.Repository;

public class DefaultRepositoryInformationAggregator : IRepositoryInformationAggregator
{
    private readonly IThreadDispatcher _dispatcher;

    public DefaultRepositoryInformationAggregator(IThreadDispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        Repositories = new ObservableCollection<RepositoryViewModel>();
    }

    public ObservableCollection<RepositoryViewModel> Repositories { get; }

    public void Add(IRepository repository, IRepositoryMonitor repositoryMonitor)
    {
        // todo at this moment, we must cast to Repository
        if (repository is not Repository repo)
        {
            throw new NotImplementedException("We expect a Repository object.");
        }

        _dispatcher.Invoke(() =>
            {
                var view = new RepositoryViewModel(repo, repositoryMonitor);

                Repositories.Remove(view);
                Repositories.Add(view);
            });
    }

    public void RemoveByPath(string path)
    {
        _dispatcher.Invoke(() =>
            {
                RepositoryViewModel[] viewsToRemove = Repositories.Where(r => r.Path.Equals(path, StringComparison.OrdinalIgnoreCase)).ToArray();

                for (var i = viewsToRemove.Length - 1; i >= 0; i--)
                {
                    Repositories.Remove(viewsToRemove[i]);
                }
            });
    }

    public string? GetStatusByPath(string path)
    {
        RepositoryViewModel? view = GetRepositoryByPath(path);
        return view?.BranchWithStatus;
    }

    private RepositoryViewModel? GetRepositoryByPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        List<RepositoryViewModel>? views = null;
        try
        {
            views = Repositories.ToList();
        }
        catch (ArgumentException)
        {
            /* there are extremely rare threading issues with System.Array.Copy() here */
        }

        var hasAny = views?.Any() ?? false;
        if (!hasAny)
        {
            return null;
        }

        if (!path.EndsWith("\\", StringComparison.Ordinal))
        {
            path += "\\";
        }

        RepositoryViewModel[] viewsByPath = views!
                                       .Where(r =>
                                           r?.Path != null
                                           &&
                                           path.StartsWith(r.Path, StringComparison.OrdinalIgnoreCase))
                                       .ToArray();

        if (!viewsByPath.Any())
        {
            return null;
        }

        return viewsByPath.OrderByDescending(r => r.Path.Length).First();
    }

    public bool HasRepository(string path)
    {
        return GetRepositoryByPath(path) != null;
    }

    public void Reset()
    {
        Repositories.Clear();
    }
}