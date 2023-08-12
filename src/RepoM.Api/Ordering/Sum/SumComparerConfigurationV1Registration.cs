namespace RepoM.Api.Ordering.Sum;

using System;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class SumComparerConfigurationV1Registration : IConfigurationRegistration
{
    public Type ConfigurationType { get; } = typeof(SumComparerConfigurationV1);

    public string Tag => SumComparerConfigurationV1.TYPE_VALUE;
}