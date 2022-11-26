namespace RepoM.Api.Ordering.Az;

using System;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class AlphabetComparerConfigurationV1Registration : IConfigurationRegistration
{
    public Type ConfigurationType { get; } = typeof(AlphabetComparerConfigurationV1);

    public string Tag { get; } = "az-comparer@1";
}