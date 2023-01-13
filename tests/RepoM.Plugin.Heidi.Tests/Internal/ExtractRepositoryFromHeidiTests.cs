namespace RepoM.Plugin.Heidi.Tests.Internal;

using System.Threading.Tasks;
using FluentAssertions;
using RepoM.Plugin.Heidi.Internal;
using RepoM.Plugin.Heidi.Internal.Config;
using VerifyTests;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class ExtractRepositoryFromHeidiTests
{
    private readonly ExtractRepositoryFromHeidi _sut;
    private readonly VerifySettings _verifySettings;

    public ExtractRepositoryFromHeidiTests()
    {
        _sut = ExtractRepositoryFromHeidi.Instance;
        _verifySettings = new VerifySettings();
        _verifySettings.UseDirectory("Verified");
    }

    [Theory]
    [InlineData(" #tagX dummy #tagX dummy2 #repo:required")]
    [InlineData(" #tagX  #repo:required #tagX")]
    [InlineData(" #tagX #tagX  #repo:required dummy")]
    [InlineData("#tagX dummy #tagX  #repo:required dummy2")]
    [InlineData("#tagX #tagX  #repo:required")]
    [InlineData("#tagX dummy #tagX     #repo:required")]
    public void TryExtract_ShouldDistinctTags_WhenDoubles(string comment)
    {
        // arrange
        var dbSettings = new HeidiSingleDatabaseConfiguration("dummy")
            {
                Comment = comment,
            };

        // act
        var result = _sut.TryExtract(dbSettings, out RepoHeidi? repo);

        // assert
        result.Should().BeTrue();
        repo!.Value.Tags.Should().BeEquivalentTo("tagX");
        repo!.Value.Tags.Length.Should().Be(1);
    }
    
    [Theory]
    [InlineData(" #tAgX dummy #tagX dummy2 #repo:required")]
    public void TryExtract_ShouldListMultipleTags(string comment)
    {
        // arrange
        var dbSettings = new HeidiSingleDatabaseConfiguration("dummy")
            {
                Comment = comment,
            };

        // act
        var result = _sut.TryExtract(dbSettings, out RepoHeidi? repo);

        // assert
        result.Should().BeTrue();
        repo!.Value.Tags.Should().BeEquivalentTo("tagX", "tAgX");
        repo!.Value.Tags.Length.Should().Be(2);
    }
    
    [Theory]
    [InlineData(" tAgX dummy # tagX #repo:required")]
    [InlineData(" #repo:required")]
    [InlineData("   #repo:required")]
    [InlineData("  ##tag  #repo:required")] // invalid double hash
    [InlineData("##tag  #repo:required")] // invalid double hash
    [InlineData("  #t@ag  #repo:required")] // invalid char in tag
    [InlineData("#t@ag  #repo:required")] // invalid char in tag
    [InlineData("#tag;  #repo:required")] // invalid char in tag
    [InlineData("#\"tag\"  #repo:required")] // invalid char , no quotes in tags
    public void TryExtract_ShouldHaveNoTags_WhenInputHasNoTags(string comment)
    {
        // arrange
        var dbSettings = new HeidiSingleDatabaseConfiguration("dummy")
            {
                Comment = comment,
            };

        // act
        var result = _sut.TryExtract(dbSettings, out RepoHeidi? repo);

        // assert
        result.Should().BeTrue();
        repo!.Value.Tags.Should().BeEmpty();
    }
    
    [Theory]
    [InlineData("#order:10 dummy #repo:required", 10)]
    [InlineData("#order:16 dummy #repo:required", 16)]
    [InlineData("#order:1 #oder:2 #order:3 use first order  #repo:required", 1)]
    [InlineData("#order:006 order with leading zeros #repo:required", 6)]
    [InlineData("#ORDER:17 capital order #repo:required", 17)]
    [InlineData("#OrdER:179 capital order #repo:required", 179)]
    public void TryExtract_ShouldUseOrder_WhenInputHasOrder(string comment, int expectedOrder)
    {
        // arrange
        var dbSettings = new HeidiSingleDatabaseConfiguration("dummy")
            {
                Comment = comment,
            };

        // act
        var result = _sut.TryExtract(dbSettings, out RepoHeidi? repo);

        // assert
        result.Should().BeTrue();
        repo!.Value.Order.Should().Be(expectedOrder);
    }

    [Theory]
    [InlineData("#order: 10 space before #repo:required")]
    [InlineData("#order16 no : #repo:required")]
    [InlineData("#order:-1 negatives not allowed #repo:required")]
    [InlineData("#order:6.54 decimals not allowed #repo:required")]
    [InlineData("#order:+17 sign not allowed #repo:required")]
    [InlineData("#order:17s invalid character #repo:required")]
    [InlineData("x#order:17 no space before hashtag #repo:required")]
    [InlineData("#order:\"17\" no quotes allowed #repo:required")]
    [InlineData("#order:999999999999999999999999999 invalid integer #repo:required")]
    public void TryExtract_ShouldUseMaxInt_WhenInputHasInvalidOrder(string comment)
    {
        // arrange
        var dbSettings = new HeidiSingleDatabaseConfiguration("dummy")
            {
                Comment = comment,
            };

        // act
        var result = _sut.TryExtract(dbSettings, out RepoHeidi? repo);

        // assert
        result.Should().BeTrue();
        repo!.Value.Order.Should().Be(int.MaxValue);
    }
    
    [Theory]
    [InlineData("#name:abc lower case #repo:required", "abc")]
    [InlineData("#name:abc #name:def fist name #repo:required", "abc")]
    [InlineData("#name:AbC capitals #repo:required", "AbC")]
    [InlineData("#name:\"Abc\" quotes #repo:required", "Abc")]
    [InlineData("#name:\"  Abc  \" spaces are trimmed #repo:required", "Abc")]
    [InlineData("#name:\"  A b c \" spaces are trimmed #repo:required", "A b c")]
    [InlineData("#name:\"0123456789-_\" valid chars #repo:required", "0123456789-_")]
    public void TryExtract_ShouldUseName_WhenInputHasName(string comment, string expectedName)
    {
        // arrange
        var dbSettings = new HeidiSingleDatabaseConfiguration("dummy")
        {
            Comment = comment,
        };

        // act
        var result = _sut.TryExtract(dbSettings, out RepoHeidi? repo);

        // assert
        result.Should().BeTrue();
        repo!.Value.Name.Should().Be(expectedName);
    }

    // [InlineData("#name:-1 #repo:required")]

    [Theory]
    [InlineData("#name: #tag:x no name / space before #repo:required")]
    [InlineData("#name:\"abd #tag:x no ending quote / space before #repo:required")]
    public void TryExtract_ShouldUseEmptyName_WhenInputHasInvalidName(string comment)
    {
        // arrange
        var dbSettings = new HeidiSingleDatabaseConfiguration("dummy")
        {
            Comment = comment,
        };

        // act
        var result = _sut.TryExtract(dbSettings, out RepoHeidi? repo);

        // assert
        result.Should().BeTrue();
        repo!.Value.Name.Should().BeEmpty();
    }

    [Theory]
    [InlineData("#repo:abc lower case", "abc")]
    [InlineData("#repo:abc #repo:def fist name", "abc")]
    [InlineData("#repo:AbC capitals", "AbC")]
    [InlineData("#repo:\"Abc\" quotes", "Abc")]
    [InlineData("#repo:\"  Abc  \" spaces are trimmed", "Abc")]
    [InlineData("#repo:\"  A b c \" spaces are trimmed", "A b c")]
    [InlineData("#repo:\"0123456789-_\" valid chars", "0123456789-_")]
    public void TryExtract_ShouldUseRepo_WhenInputHasRepo(string comment, string expectedName)
    {
        // arrange
        var dbSettings = new HeidiSingleDatabaseConfiguration("dummy")
            {
                Comment = comment,
            };

        // act
        var result = _sut.TryExtract(dbSettings, out RepoHeidi? repo);

        // assert
        result.Should().BeTrue();
        repo!.Value.Repository.Should().Be(expectedName);
    }

    [Theory]
    [InlineData("#repo: repo cannot start with space")]
    [InlineData("#repo:abc* invalid char")]
    public void TryExtract_ShouldReturnFalse_WhenRepoIsNotValid(string comment)
    {
        // arrange
        var dbSettings = new HeidiSingleDatabaseConfiguration("dummy")
            {
                Comment = comment,
            };

        // act
        var result = _sut.TryExtract(dbSettings, out RepoHeidi? _);

        // assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task TryExtract_ShouldExtractData_WhenAvailableInComment()
    {
        // arrange
        var dbSettings = new HeidiSingleDatabaseConfiguration("hk")
            {
                Comment = "abc #repo:\"repom 234\" #order:42 #name:name123 rubbischsdfkl$934 #T xsf",
            };

        // act
        var result = _sut.TryExtract(dbSettings, out RepoHeidi? repo);

        // assert
        await Verifier.Verify(new
            {
                Success = result,
                Repository = repo,
            },
            _verifySettings);
    }
}