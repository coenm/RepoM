namespace Yaml.Poc.Tests;

using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using RepoM.Core.Plugin.RepositoryOrdering;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using RepoM.Core.Plugin.RepositoryOrdering.Implementations.Az;
using RepoM.Core.Plugin.RepositoryOrdering.Implementations.IsPinned;
using RepoM.Core.Plugin.RepositoryOrdering.Implementations.Score;
using RepoM.Core.Plugin.RepositoryOrdering.Implementations.Sum;
using SimpleInjector;
using VerifyXunit;
using Xunit;
using Yaml.Poc.Tests.DI;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

[UsesVerify]
public class YamlTests
{
    private readonly string yml = @"
  !sum-comparer@1
  comparers:
  - !az-comparer@1
    property: Name
    weight: 5
  - !score-comparer@1
    score-provider: 
      !is-pinned-scorer@1
      weight: 5
";

    //https://github.com/aaubry/YamlDotNet/wiki/Serialization.Deserializer
    [Fact]
    public async Task Abc()
    {
        IDeserializer deserializer = new DeserializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .WithTagMapping("!score-comparer@1", typeof(ScoreComparerConfigurationV1))
            .WithTagMapping("!az-comparer@1", typeof(AlphabetComparerConfigurationV1))
            .WithTagMapping("!sum-comparer@1", typeof(SumComparerConfigurationV1))
            .WithTagMapping("!is-pinned-scorer@1", typeof(IsPinnedScorerConfigurationV1))
            .Build();

        var result = deserializer.Deserialize<IRepositoriesComparerConfiguration>(yml);

        await Verifier.Verify(result);
    }


    [Fact]
    public async Task DI()
    {
        var container = new Container();
        Bootstrapper.RegisterOrderingConfiguration(container);
        container.Verify();

        DeserializerBuilder builder = new DeserializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance);

        foreach (IConfigurationRegistration instance in container.GetAllInstances<IConfigurationRegistration>())
        {
            var tag = instance.Tag.TrimStart('!');
            builder.WithTagMapping("!" + tag, instance.ConfigurationType);
        }

        IDeserializer deserializer = builder.Build();


        IRepositoriesComparerConfiguration result = deserializer.Deserialize<IRepositoriesComparerConfiguration>(yml);
        // var instance2s = container.GetAllInstances<IRepositoryScoreCalculatorFactory<>>();
        // IRepositoryComparerFactory
         await Verifier.Verify(result).UseMethodName(nameof(Abc));
    }
}