namespace RepoM.Api.Ordering.Composition;

using System;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class CompositionComparerConfigurationV1Registration : IConfigurationRegistration
{
    public Type ConfigurationType { get; } = typeof(CompositionComparerConfigurationV1);

    public string Tag => CompositionComparerConfigurationV1.TYPE_VALUE;
}