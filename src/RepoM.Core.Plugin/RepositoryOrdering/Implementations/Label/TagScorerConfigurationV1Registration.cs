namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations.Label;

using System;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class TagScorerConfigurationV1Registration : IConfigurationRegistration
{
    public Type ConfigurationType { get; } = typeof(TagScorerConfigurationV1);

    public string Tag { get; } = "tag-scorer@1";
}