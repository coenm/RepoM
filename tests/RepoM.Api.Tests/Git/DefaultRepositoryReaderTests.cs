namespace RepoM.Api.Tests.Git;

using System;
using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using Xunit;

public class DefaultRepositoryReaderTests
{
    private readonly IRepositoryTagsFactory _resolver = A.Fake<IRepositoryTagsFactory>();
    private readonly ILogger _logger = A.Dummy<ILogger>();

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<DefaultRepositoryReader> act1 = () => new DefaultRepositoryReader(_resolver, null!);
        Func<DefaultRepositoryReader> act2 = () => new DefaultRepositoryReader(null!, _logger);

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ReadRepositoryAsync_ShouldReturnNull_WhenPathIsNullOrEmpty(string? path)
    {
        // arrange
        var sut = new DefaultRepositoryReader(_resolver, _logger);

        // act
        Repository? result = await sut.ReadRepositoryAsync(path!);

        // assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ReadRepositoryAsync_ShouldReturnNull_WhenPathDoesNotExist()
    {
        // arrange
        var path = Path.Combine("C:", Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        var sut = new DefaultRepositoryReader(_resolver, _logger);

        // act
        Repository? result = await sut.ReadRepositoryAsync(path);

        // assert
        result.Should().BeNull(because:$"Path '{path}' is generated and should not exists.");
    }
}