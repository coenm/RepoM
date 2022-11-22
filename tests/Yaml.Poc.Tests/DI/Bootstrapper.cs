namespace Yaml.Poc.Tests.DI;

using RepoM.Core.Plugin.RepositoryOrdering;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using RepoM.Core.Plugin.RepositoryOrdering.Implementations.Az;
using RepoM.Core.Plugin.RepositoryOrdering.Implementations.Composition;
using RepoM.Core.Plugin.RepositoryOrdering.Implementations.IsPinned;
using RepoM.Core.Plugin.RepositoryOrdering.Implementations.Label;
using RepoM.Core.Plugin.RepositoryOrdering.Implementations.Score;
using RepoM.Core.Plugin.RepositoryOrdering.Implementations.Sum;
using SimpleInjector;
using Container = SimpleInjector.Container;

internal static class Bootstrapper
{
    public static void RegisterOrderingConfiguration(Container container)
    {
        container.Collection.Append<IConfigurationRegistration, AlphabetComparerConfigurationV1Registration>(Lifestyle.Singleton);
        container.Collection.Append<IRepositoryComparerFactory<AlphabetComparerConfigurationV1>, AzRepositoryComparerFactory>(Lifestyle.Singleton);

        container.Collection.Append<IConfigurationRegistration, CompositionComparerConfigurationV1Registration>(Lifestyle.Singleton);
        container.Collection.Append<IRepositoryComparerFactory<CompositionComparerConfigurationV1>, CompositionRepositoryComparerFactory>(Lifestyle.Singleton);

        container.Collection.Append<IConfigurationRegistration, IsPinnedScorerConfigurationV1Registration>(Lifestyle.Singleton);
        container.Collection.Append<IRepositoryScoreCalculatorFactory<IsPinnedScorerConfigurationV1>, IsPinnedScorerFactory>(Lifestyle.Singleton);

        container.Collection.Append<IConfigurationRegistration, LabelScorerConfigurationV1Registration>(Lifestyle.Singleton);
        container.Collection.Append<IRepositoryScoreCalculatorFactory<LabelScorerConfigurationV1>, LabelScorerFactory>(Lifestyle.Singleton);

        container.Collection.Append<IConfigurationRegistration, ScoreComparerConfigurationV1Registration>(Lifestyle.Singleton);
        container.Collection.Append<IRepositoryComparerFactory<ScoreComparerConfigurationV1>, ScoreRepositoryComparerFactory>(Lifestyle.Singleton);

        container.Collection.Append<IConfigurationRegistration, SumComparerConfigurationV1Registration>(Lifestyle.Singleton);
        container.Collection.Append<IRepositoryComparerFactory<SumComparerConfigurationV1>, SumRepositoryComparerFactory>(Lifestyle.Singleton);
    }
}