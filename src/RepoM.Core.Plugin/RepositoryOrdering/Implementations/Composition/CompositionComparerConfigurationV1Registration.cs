namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations.Composition;

using System;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class CompositionComparerConfigurationV1Registration : IConfigurationRegistration
{
    public Type ConfigurationType { get; } = typeof(CompositionComparerConfigurationV1);

    public string Tag { get; } = "xx@1";
}