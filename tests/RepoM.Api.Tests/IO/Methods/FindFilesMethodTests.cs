namespace RepoM.Api.Tests.IO.Methods;

using FakeItEasy;
using RepoM.Api.Common;
using RepoM.Api.IO.Methods;
using RepoM.Core.Plugin.RepositoryFinder;
using System;
using FluentAssertions;
using Xunit;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;

public class FindFilesMethodTests
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    public FindFilesMethodTests()
    {
        _fileSystem = A.Fake<IFileSystem>();
        _logger = A.Fake<ILogger>();
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<FindFilesMethod> act1 = () => new FindFilesMethod(A.Dummy<IFileSystem>(), null!);
        Func<FindFilesMethod> act2 = () => new FindFilesMethod(null!, A.Dummy<ILogger>());

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("FindFiles", true)]
    [InlineData("findfiles", true)]
    [InlineData("FindFilesx", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void CanHandle_ShouldReturnExpected(string? method, bool expectedResult)
    {
        // arrange
        var sut = new FindFilesMethod(_fileSystem, _logger);

        // act
        var result = sut.CanHandle(method!);

        // assert
        _ = result.Should().Be(expectedResult);
    }
}