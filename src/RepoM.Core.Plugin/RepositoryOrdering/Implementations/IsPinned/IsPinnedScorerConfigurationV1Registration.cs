namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations.IsPinned;

using System;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class IsPinnedScorerConfigurationV1Registration : IConfigurationRegistration
{
    public Type ConfigurationType { get; } = typeof(IsPinnedScorerConfigurationV1);

    public string Tag { get; } = "is-pinned-scorer@1";
}