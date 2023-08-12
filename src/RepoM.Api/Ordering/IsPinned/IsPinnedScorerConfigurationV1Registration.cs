namespace RepoM.Api.Ordering.IsPinned;

using System;
using JetBrains.Annotations;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

[UsedImplicitly]
public class IsPinnedScorerConfigurationV1Registration : IConfigurationRegistration
{
    public Type ConfigurationType { get; } = typeof(IsPinnedScorerConfigurationV1);

    public string Tag => IsPinnedScorerConfigurationV1.TYPE_VALUE;
}