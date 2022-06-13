namespace RepoM.Api.Common.Common;

using System;
using System.Collections.Generic;
using RepoM.Api.Common.Git.AutoFetch;

public interface IAppSettingsService
{
    AutoFetchMode AutoFetchMode { get; set; }

    bool PruneOnFetch { get; set; }

    double MenuWidth { get; set; }

    double MenuHeight { get; set; }

    List<string> EnabledSearchProviders { get; set; }

    void RegisterInvalidationHandler(Action handler);
}