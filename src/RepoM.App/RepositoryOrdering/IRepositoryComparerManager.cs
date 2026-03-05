namespace RepoM.App.RepositoryOrdering;

using System;
using System.Collections.Generic;
using RepoM.Api.Git;

public interface IRepositoryComparerManager
{
    event EventHandler<string>? SelectedRepositoryComparerKeyChanged;

    IObservable<IComparer<RepositoryViewModel>> SortObservable { get; }

    IReadOnlyList<string> RepositoryComparerKeys { get; }

    string SelectedRepositoryComparerKey { get; }

    bool SetRepositoryComparer(string key);
}
