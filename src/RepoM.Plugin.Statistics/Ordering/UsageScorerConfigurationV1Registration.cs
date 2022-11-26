namespace RepoM.Plugin.Statistics.Ordering;

using System;
using JetBrains.Annotations;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

[UsedImplicitly]
public class UsageScorerConfigurationV1Registration : IConfigurationRegistration
{
    public Type ConfigurationType { get; } = typeof(UsageScorerConfigurationV1);

    public string Tag { get; } = "usage@1";
}