namespace RepoM.Plugin.Heidi.Tests.Internal;

using System;
using FluentAssertions;
using RepoM.Plugin.Heidi.Internal;
using RepoM.Plugin.Heidi.Internal.Config;
using Xunit;

public class ExtractRepositoryFromHeidiTests
{
    private readonly ExtractRepositoryFromHeidi _sut;

    public ExtractRepositoryFromHeidiTests()
    {
        _sut = new ExtractRepositoryFromHeidi();
    }

    [Fact]
    public void Abc()
    {
        // arrange

        // act
        var result = _sut.TryExtract(new HeidiSingleDatabaseConfiguration()
            {
                Key = "hk",
                Comment = "abc #repo:repom #order:12 #name:name123",
            },
            out RepoHeidi? repo);

        // assert
        result.Should().BeTrue();
        repo.Should().Be(new RepoHeidi()
            {
                HeidiKey = "hk",
                Order = 12,
                RepositoryName = "repom",
                Tags = Array.Empty<string>(),
            });
    }
}