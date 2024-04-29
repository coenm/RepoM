namespace RepoM.Api.Git;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.Logging;
using RepoM.Api.Common;
using RepoM.Core.Plugin.Repository;

public class DefaultRepositoryInformationAggregator : IRepositoryInformationAggregator
{
    private readonly IThreadDispatcher _dispatcher;
    private readonly ILogger _logger;

    public DefaultRepositoryInformationAggregator(IThreadDispatcher dispatcher, ILogger logger)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Repositories = new ObservableCollection<RepositoryViewModel>();
    }

    public ObservableCollection<RepositoryViewModel> Repositories { get; }

    public void Add(IRepository repository, IRepositoryMonitor repositoryMonitor)
    {
        // GitHub issue: https://github.com/coenm/RepoM/issues/90
        // at this moment, we must cast to Repository
        if (repository is not Repository repo)
        {
            throw new NotImplementedException("We expect a Repository object.");
        }

        _dispatcher.Invoke(() =>
            {
                _logger.LogTrace("DefaultRepositoryInformationAggregator Add repository {Name}", repo.Name);
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
            views = [.. Repositories, ];
        }
        catch (ArgumentException)
        {
            /* there are extremely rare threading issues with System.Array.Copy() here */
        }

        if (views == null)
        {
            return null;
        }

        var hasAny = views.Count > 0;
        if (!hasAny)
        {
            return null;
        }

        if (!path.EndsWith('\\'))
        {
            path += "\\";
        }

        RepositoryViewModel[] viewsByPath = views
           .Where(r =>
               r.Path != null
               &&
               path.StartsWith(r.Path, StringComparison.OrdinalIgnoreCase))
           .ToArray();

        if (viewsByPath.Length == 0)
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