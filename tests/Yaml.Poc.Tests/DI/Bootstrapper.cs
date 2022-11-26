namespace Yaml.Poc.Tests.DI;

using System;
using System.Threading.Tasks;
using RepoM.Api.Git;
using RepoM.Api.Ordering.Az;
using RepoM.Api.Ordering.Composition;
using RepoM.Api.Ordering.IsPinned;
using RepoM.Api.Ordering.Label;
using RepoM.Api.Ordering.Score;
using RepoM.Api.Ordering.Sum;
using RepoM.Core.Plugin.RepositoryOrdering;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using SimpleInjector;
using Container = SimpleInjector.Container;


internal class RepositoryComparerCompositionFactory : IRepositoryComparerFactory
{
    private readonly Container _container;

    public RepositoryComparerCompositionFactory(Container container)
    {
        _container = container;
    }

    public IRepositoryComparer Create(IRepositoriesComparerConfiguration configuration)
    {
        Type type = typeof(IRepositoryComparerFactory<>).MakeGenericType(configuration.GetType());
        dynamic factory = _container.GetInstance(type);
        return factory.Create((dynamic)configuration);
    }
}

internal class RepositoryScoreCalculatorFactory : IRepositoryScoreCalculatorFactory
{
    private readonly Container _container;

    public RepositoryScoreCalculatorFactory(Container container)
    {
        _container = container;
    }

    public IRepositoryScoreCalculator Create(IRepositoryScorerConfiguration configuration)
    {
        Type type = typeof(IRepositoryScoreCalculatorFactory<>).MakeGenericType(configuration.GetType());
        dynamic factory = _container.GetInstance(type);
        return factory.Create((dynamic)configuration);
    }

}

internal class DummyIRepositoryMonitor : IRepositoryMonitor
{
    public event EventHandler<Repository>? OnChangeDetected;
    public event EventHandler<string>? OnDeletionDetected;
    public event EventHandler<bool>? OnScanStateChanged;
    public void Stop()
    {
        throw new NotImplementedException();
    }

    public void Observe()
    {
        throw new NotImplementedException();
    }

    public void Reset()
    {
        throw new NotImplementedException();
    }

    public Task ScanForLocalRepositoriesAsync()
    {
        throw new NotImplementedException();
    }

    public void IgnoreByPath(string path)
    {
        throw new NotImplementedException();
    }

    public void SetPinned(bool newValue, Repository repository)
    {
        throw new NotImplementedException();
    }

    public bool IsPinned(Repository repository)
    {
        throw new NotImplementedException();
    }
}

internal static class Bootstrapper
{
    public static void RegisterOrderingConfiguration(Container container)
    {
        container.RegisterSingleton<IRepositoryComparerFactory, RepositoryComparerCompositionFactory>();
        container.RegisterSingleton<IRepositoryScoreCalculatorFactory, RepositoryScoreCalculatorFactory>();

        container.Collection.Append<IConfigurationRegistration, AlphabetComparerConfigurationV1Registration>(Lifestyle.Singleton);
        container.Collection.Append<IRepositoryComparerFactory<AlphabetComparerConfigurationV1>, AzRepositoryComparerFactory>(Lifestyle.Singleton);
        container.Register<IRepositoryComparerFactory<AlphabetComparerConfigurationV1>, AzRepositoryComparerFactory>(Lifestyle.Singleton);
        
        container.Collection.Append<IConfigurationRegistration, CompositionComparerConfigurationV1Registration>(Lifestyle.Singleton);
        container.Collection.Append<IRepositoryComparerFactory<CompositionComparerConfigurationV1>, CompositionRepositoryComparerFactory>(Lifestyle.Singleton);
        container.Register<IRepositoryComparerFactory<CompositionComparerConfigurationV1>, CompositionRepositoryComparerFactory>(Lifestyle.Singleton);

        container.Register<IRepositoryMonitor, DummyIRepositoryMonitor>(Lifestyle.Singleton);
        container.Collection.Append<IConfigurationRegistration, IsPinnedScorerConfigurationV1Registration>(Lifestyle.Singleton);
        container.Collection.Append<IRepositoryScoreCalculatorFactory<IsPinnedScorerConfigurationV1>, IsPinnedScorerFactory>(Lifestyle.Singleton);
        container.Register<IRepositoryScoreCalculatorFactory<IsPinnedScorerConfigurationV1>, IsPinnedScorerFactory>(Lifestyle.Singleton);

        container.Collection.Append<IConfigurationRegistration, TagScorerConfigurationV1Registration>(Lifestyle.Singleton);
        container.Collection.Append<IRepositoryScoreCalculatorFactory<TagScorerConfigurationV1>, TagScorerFactory>(Lifestyle.Singleton);
        container.Register<IRepositoryScoreCalculatorFactory<TagScorerConfigurationV1>, TagScorerFactory>(Lifestyle.Singleton);

        container.Collection.Append<IConfigurationRegistration, ScoreComparerConfigurationV1Registration>(Lifestyle.Singleton);
        container.Collection.Append<IRepositoryComparerFactory<ScoreComparerConfigurationV1>, ScoreRepositoryComparerFactory>(Lifestyle.Singleton);
        container.Register<IRepositoryComparerFactory<ScoreComparerConfigurationV1>, ScoreRepositoryComparerFactory>(Lifestyle.Singleton);

        container.Collection.Append<IConfigurationRegistration, SumComparerConfigurationV1Registration>(Lifestyle.Singleton);
        container.Collection.Append<IRepositoryComparerFactory<SumComparerConfigurationV1>, SumRepositoryComparerFactory>(Lifestyle.Singleton);
        container.Register<IRepositoryComparerFactory<SumComparerConfigurationV1>, SumRepositoryComparerFactory>(Lifestyle.Singleton);
    }
}