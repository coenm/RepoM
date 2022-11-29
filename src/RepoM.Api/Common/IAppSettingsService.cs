namespace RepoM.Api.Common;

using System;
using System.Collections.Generic;
using RepoM.Api.Git.AutoFetch;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public interface ICompareSettingsService
{
    IRepositoriesComparerConfiguration Configuration { get; }
}

public interface IAppSettingsService
{
    AutoFetchMode AutoFetchMode { get; set; }

    bool PruneOnFetch { get; set; }

    double MenuWidth { get; set; }

    double MenuHeight { get; set; }

    List<string> EnabledSearchProviders { get; set; }

    string SonarCloudPersonalAccessToken { get; set; }

    string AzureDevOpsPersonalAccessToken { get; set; }

    string AzureDevOpsBaseUrl { get; set; }

    string SortKey { get; set; }

    void RegisterInvalidationHandler(Action handler);
}
