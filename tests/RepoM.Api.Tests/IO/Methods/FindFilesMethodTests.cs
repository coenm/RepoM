namespace RepoM.Api.Tests.IO.Methods;

using FakeItEasy;
using RepoM.Api.IO.Methods;
using System;
using FluentAssertions;
using Xunit;
using System.IO.Abstractions;
using System.Linq;
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

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Handle_ShouldReturnNull_WhenArgsLengthIsLessThenTwo(int length)
    {
        // arrange
        var args = Enumerable.Range(0, length).Select(x => $"{x}").Cast<object>().ToArray();
        var sut = new FindFilesMethod(_fileSystem, _logger);

        // act
        object? result = sut.Handle("FindFiles", args);

        // assert
        _ = result.Should().BeNull();
    }

    [Fact]
    public void Handle_ShouldReturnNull_WhenFirstArgumentIsNotString()
    {
        // arrange
        var args = new object[] { 1, "dummy", };
        var sut = new FindFilesMethod(_fileSystem, _logger);

        // act
        object? result = sut.Handle("FindFiles", args);

        // assert
        _ = result.Should().BeNull();
    }

    [Fact]
    public void Handle_ShouldReturnNull_WhenSecondArgumentIsNotString()
    {
        // arrange
        var args = new object[] {"dummy", 42, };
        var sut = new FindFilesMethod(_fileSystem, _logger);

        // act
        object? result = sut.Handle("FindFiles", args);

        // assert
        _ = result.Should().BeNull();
    }
}