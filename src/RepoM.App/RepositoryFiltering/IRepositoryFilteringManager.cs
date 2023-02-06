namespace RepoM.App.RepositoryFiltering;

using System;
using System.Collections;
using System.Collections.Generic;
using RepoM.Api.Git;
using RepoM.Core.Plugin.RepositoryFiltering;

public interface IRepositoryFilteringManager
{
    event EventHandler<string>? SelectedQueryParserChanged;

    IQueryParser QueryParser { get; }

    IReadOnlyList<string> QueryParserKeys { get; }

    string SelectedQueryParserKey { get; }

    bool SetQueryParser(string key);
}