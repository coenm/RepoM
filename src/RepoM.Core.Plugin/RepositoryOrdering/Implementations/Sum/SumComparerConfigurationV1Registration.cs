namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations.Sum;

using System;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class SumComparerConfigurationV1Registration : IConfigurationRegistration
{
    public Type ConfigurationType { get; } = typeof(SumComparerConfigurationV1);

    public string Tag { get; } = "sum-comparer@1";
}