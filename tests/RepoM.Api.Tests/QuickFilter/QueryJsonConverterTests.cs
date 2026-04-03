namespace RepoM.Api.Tests.QuickFilter;

using System;
using AwesomeAssertions;
using Newtonsoft.Json;
using RepoM.Api.QuickFilter;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;
using Xunit;

public class QueryJsonConverterTests
{
    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        Converters = { new QueryJsonConverter(), },
    };

    [Fact]
    public void SerializeAndDeserialize_ShouldRoundTripComplexQuery()
    {
        // arrange
        IQuery query = new AndQuery(
            new SimpleTerm("tag", "work"),
            new OrQuery(new FreeText("github"), TrueQuery.Instance),
            new NotQuery(new StartsWithTerm("branch", "ma")));

        // act
        var json = JsonConvert.SerializeObject(query, SerializerSettings);
        var result = JsonConvert.DeserializeObject<IQuery>(json, SerializerSettings);

        // assert
        result.Should().BeOfType<AndQuery>();
        var and = (AndQuery)result!;
        and.Items.Should().HaveCount(3);

        and.Items[0].Should().BeOfType<SimpleTerm>();
        ((SimpleTerm)and.Items[0]).Term.Should().Be("tag");
        ((SimpleTerm)and.Items[0]).Value.Should().Be("work");

        and.Items[1].Should().BeOfType<OrQuery>();
        var or = (OrQuery)and.Items[1];
        or.Items.Should().HaveCount(2);
        or.Items[0].Should().BeOfType<FreeText>();
        ((FreeText)or.Items[0]).Value.Should().Be("github");
        or.Items[1].Should().BeSameAs(TrueQuery.Instance);

        and.Items[2].Should().BeOfType<NotQuery>();
        var not = (NotQuery)and.Items[2];
        not.Item.Should().BeOfType<StartsWithTerm>();
        ((StartsWithTerm)not.Item).Term.Should().Be("branch");
        ((StartsWithTerm)not.Item).Value.Should().Be("ma");
    }

    [Fact]
    public void Serialize_ShouldWriteNull_WhenQueryIsNull()
    {
        // arrange
        IQuery? query = null;

        // act
        var json = JsonConvert.SerializeObject(query, SerializerSettings);

        // assert
        json.Should().Be("null");
    }

    [Fact]
    public void Deserialize_ShouldThrow_WhenTypeIsUnknown()
    {
        // arrange
        const string json = "{\"type\":\"unknown\"}";

        // act
        Action act = () => JsonConvert.DeserializeObject<IQuery>(json, SerializerSettings);

        // assert
        act.Should().ThrowExactly<JsonSerializationException>()
            .WithMessage("*Unknown query type 'unknown'.*");
    }
}