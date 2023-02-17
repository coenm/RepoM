namespace RepoM.App.RepositoryFiltering;

using System;
using System.Collections.Generic;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;

public interface IRepositoryFilteringManager
{
    event EventHandler<string>? SelectedQueryParserChanged;

    event EventHandler<string>? SelectedFilterChanged;

    IQueryParser QueryParser { get; }

    IQuery PreFilter { get; }

    IQuery? AlwaysVisibleFilter { get; }

    IReadOnlyList<string> QueryParserKeys { get; }

    IReadOnlyList<string> FilterKeys { get; }

    string SelectedQueryParserKey { get; }

    string SelectedFilterKey { get; }

    bool SetQueryParser(string key);

    bool SetFilter(string key);
}