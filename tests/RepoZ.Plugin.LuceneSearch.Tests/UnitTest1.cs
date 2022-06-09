namespace RepoZ.Plugin.LuceneSearch.Tests;

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using RepoZ.Plugin.LuceneSearch;
using Xunit;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        // arrange
        var sut = new RepositoryIndex(new LuceneDirectoryInstance(new RamLuceneDirectoryFactory()));

        // act
        List<RepositorySearchResult> results = sut.Search("tag:work project x", SearchOperator.Or, out var hits);

        // assert
        hits.Should().Be(0);
    }

    [Fact]
    public async Task Tesdt1()
    {
        // arrange
        var sut = new RepositoryIndex(new LuceneDirectoryInstance(new RamLuceneDirectoryFactory()));
        var item = new RepositorySearchModel(
            "c:/a/b/c",
            "RepoZ",
            new List<string>()
                {
                    "repositories",
                    "work",
                });
        await sut.ReIndexMediaFileAsync(item).ConfigureAwait(false);

        // act
        List<RepositorySearchResult> result = sut.Search("tag:work project x", SearchOperator.Or, out var hits);

        // assert
        hits.Should().Be(1);
    }
}