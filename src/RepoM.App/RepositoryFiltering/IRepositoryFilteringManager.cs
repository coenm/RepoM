namespace RepoM.App.RepositoryFiltering;

using System;
using System.Collections.Generic;
using RepoM.Api.Git;

public interface IRepositoryFilteringManager
{
    IObservable<Func<RepositoryViewModel, bool>> CreateFilterObservable(IObservable<string> textInput);

    event EventHandler<string>? SelectedQueryParserChanged;

    event EventHandler<string>? SelectedFilterChanged;

    IReadOnlyList<string> QueryParserKeys { get; }

    IReadOnlyList<string> FilterKeys { get; }

    string SelectedQueryParserKey { get; }

    string SelectedFilterKey { get; }

    bool SetQueryParser(string key);

    bool SetFilter(string key);
}
