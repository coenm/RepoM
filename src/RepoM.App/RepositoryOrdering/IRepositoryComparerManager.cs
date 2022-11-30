namespace RepoM.App.RepositoryOrdering;

using System;
using System.Collections;
using System.Collections.Generic;

public interface IRepositoryComparerManager
{
    event EventHandler<string>? SelectedRepositoryComparerKeyChanged;

    IComparer Comparer { get; }

    IReadOnlyList<string> RepositoryComparerKeys { get; }
    string SelectedRepositoryComparerKey { get; }
    bool SetRepositoryComparer(string key);
}