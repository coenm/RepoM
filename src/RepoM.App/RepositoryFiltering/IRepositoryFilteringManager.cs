namespace RepoM.App.RepositoryFiltering;

using System;
using System.Collections.Generic;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;

public interface IRepositoryFilteringManager
{
    event EventHandler<string>? SelectedQueryParserChanged;
    event EventHandler<string>? PreFilterChanged;

    IQueryParser QueryParser { get; }

    IReadOnlyList<string> QueryParserKeys { get; }

    string SelectedQueryParserKey { get; }

    bool SetQueryParser(string key);

    IQuery PreFilter { get; }
}