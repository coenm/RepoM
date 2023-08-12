namespace RepoM.Api.Ordering.Score;

using System;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class ScoreComparerConfigurationV1Registration : IConfigurationRegistration
{
    public Type ConfigurationType { get; } = typeof(ScoreComparerConfigurationV1);

    public string Tag => ScoreComparerConfigurationV1.TYPE_VALUE;
}
