namespace RepoM.Plugin.Statistics.Ordering;

using System;
using JetBrains.Annotations;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

[UsedImplicitly]
public class LastOpenedConfigurationV1Registration : IConfigurationRegistration
{
    public Type ConfigurationType { get; } = typeof(LastOpenedConfigurationV1);

    public string Tag => "last-opened-comparer@1";
}