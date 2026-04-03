namespace RepoM.Api.Tests.QuickFilter;

using AwesomeAssertions;
using Newtonsoft.Json;
using RepoM.Api.QuickFilter;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;
using Xunit;

public class QuickFilterModelTests
{
    [Fact]
    public void Ctor_ShouldInitializeDefaults()
    {
        // arrange

        // act
        var sut = new QuickFilterModel();

        // assert
        sut.Label.Should().BeEmpty();
        sut.Query.Should().BeSameAs(RepoM.Core.Plugin.RepositoryFiltering.Clause.TrueQuery.Instance);
        sut.IsActive.Should().BeFalse();
        sut.IsInverse.Should().BeFalse();
        sut.Order.Should().Be(0);
        sut.ToolTip.Should().BeEmpty();
        sut.IsBuiltIn.Should().BeFalse();
    }

    [Fact]
    public void Serialize_ShouldIgnoreIsBuiltIn()
    {
        // arrange
        var sut = new QuickFilterModel
        {
            Label = "Work",
            Query = new SimpleTerm("tag", "work"),
            IsBuiltIn = true,
        };

        // act
        var json = JsonConvert.SerializeObject(sut, new JsonSerializerSettings
        {
            Converters = { new QueryJsonConverter(), },
        });

        // assert
        json.Should().Contain("\"Label\":\"Work\"");
        json.Should().NotContain("IsBuiltIn");
    }
}