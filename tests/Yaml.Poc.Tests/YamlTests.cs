namespace Yaml.Poc.Tests;

using System.Threading.Tasks;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using VerifyXunit;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

[UsesVerify]
public class YamlTests
{
    //https://github.com/aaubry/YamlDotNet/wiki/Serialization.Deserializer
    [Fact]
    public async Task Abc()
    {
        var yml = @"
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
}