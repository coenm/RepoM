namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations.Label;

using System;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class LabelScorerConfigurationV1Registration : IConfigurationRegistration
{
    public Type ConfigurationType { get; } = typeof(LabelScorerConfigurationV1);

    public string Tag { get; } = "label@1";
}