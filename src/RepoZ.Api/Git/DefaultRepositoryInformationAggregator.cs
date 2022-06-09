namespace RepoZ.Api.Git;

using RepoZ.Api.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public class DefaultRepositoryInformationAggregator : IRepositoryInformationAggregator
{
    private readonly IThreadDispatcher _dispatcher;

    public DefaultRepositoryInformationAggregator(IThreadDispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        Repositories = new ObservableCollection<RepositoryView>();
    }

    public ObservableCollection<RepositoryView> Repositories { get; }

    public void Add(Repository repository)
    {
        _dispatcher.Invoke(() =>
            {
                var view = new RepositoryView(repository);

                Repositories.Remove(view);
                Repositories.Add(view);
            });
    }

    public void RemoveByPath(string path)
    {
        _dispatcher.Invoke(() =>
            {
                RepositoryView[] viewsToRemove = Repositories.Where(r => r.Path.Equals(path, StringComparison.OrdinalIgnoreCase)).ToArray();

                for (var i = viewsToRemove.Length - 1; i >= 0; i--)
                {
                    Repositories.Remove(viewsToRemove[i]);
                }
            });
    }

    public string? GetStatusByPath(string path)
    {
        RepositoryView? view = GetRepositoryByPath(path);
        return view?.BranchWithStatus;
    }

    private RepositoryView? GetRepositoryByPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        List<RepositoryView>? views = null;
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

        RepositoryView[] viewsByPath = views!
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