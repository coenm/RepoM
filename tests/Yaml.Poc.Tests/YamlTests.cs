namespace Yaml.Poc.Tests;

using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Serialization.NodeTypeResolvers;


/// <summary>
/// Configuration to compare two repos
/// </summary>
public interface ICompareReposData
{
}

// SumCompositionComparer@1
public class SumCompositionComparerV1Data : ICompareReposData
{
    public List<ICompareReposData> Items { get; set; }
}

// ScoreComparer@1
public class ScoreComparerV1Data : ICompareReposData
{
    public IScoreRepoData Score { get; set; }
}

// AlphabetComparer@1
public class AlphabetComparerV1Data : ICompareReposData
{
    public string Property { get; set; }

    public int Weight { get; set; }
}

/// <summary>
/// Configuration to score a single repo
/// </summary>
public interface IScoreRepoData
{
}

// IsPinnedScorer@1
public class IsPinnedScorerV1Data : IScoreRepoData
{
    public int Weight { get; set; }
}




public class YamlTests
{
    //https://github.com/aaubry/YamlDotNet/wiki/Serialization.Deserializer
    [Fact]
    public void Abc()
    {
        string yml1 = @"
- !contact@1
  Name: Oz-Ware
  PhoneNumber: 123456789
- !repo-score-provider@1
  Score:
  - provider: isPinned@1
    weight: 5";

        string yml = @"
  !sum-comparer@1
  items:
  - !alphabet-comparer@1
    property: Name
    weight: 5
  - !score-comparer@1
    score: 
      !is-pinned-scorer@1
      weight: 5
";

        var deserializer = new DeserializerBuilder()
                           .WithNamingConvention(HyphenatedNamingConvention.Instance)
                           .WithTagMapping("!score-comparer@1", typeof(ScoreComparerV1Data))
                           .WithTagMapping("!alphabet-comparer@1", typeof(AlphabetComparerV1Data))
                           .WithTagMapping("!is-pinned-scorer@1", typeof(IsPinnedScorerV1Data))


                           // .WithTagMapping("!repo-score-provider@1", typeof(RepoScoreProvider))
                           // .WithAttributeOverride<Contact>(
                           //     c => c.Name,
                           //     new YamlMemberAttribute
                           //         {
                           //             Alias = "full_name",
                           //         })
                           // .WithTypeConverter<Coen>(wrapped =>
                           //     {
                           //         return new Coen(wrapped);
                           //     },
                           //     syntax => { })
                           // .WithTypeInspector(inspector =>
                           //     {
                           //         return inspector;
                           //     })
                           // .WithNodeDeserializer(
                           //     inner => new ValidatingNodeDeserializer(inner),
                           //     s => s.InsteadOf<ObjectNodeDeserializer>()
                           // )
                           // .WithNodeTypeResolver(inner => new SystemTypeFromTagNodeTypeResolver(inner),
                           //     s => s.InsteadOf<DefaultContainersNodeTypeResolver>()
                           //  )
                           .Build();

        var result = deserializer.Deserialize<ICompareReposData>(yml);



    }
}

public class SystemTypeFromTagNodeTypeResolver : INodeTypeResolver
{
    public SystemTypeFromTagNodeTypeResolver(INodeTypeResolver inner)
    {
        //throw new NotImplementedException();
    }

    public bool Resolve(NodeEvent nodeEvent, ref Type currentType)
    {
        if (nodeEvent.Tag.Value.StartsWith("!clr:"))
        {
            var netTypeName = nodeEvent.Tag.Value.Substring(5);
            var type = Type.GetType(netTypeName);
            if (type != null)
            {
                currentType = type;
                return true;
            }
        }

        return false;
    }
}


