namespace RepoM.Api.Tests;

using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using RepoM.Core.Plugin.Common;
using Xunit;

public class EnsureStartupTests
{
    private readonly IFileSystem _fileSystem = A.Fake<IFileSystem>();
    private readonly IAppDataPathProvider _appDataProvider = A.Fake<IAppDataPathProvider>();
    private readonly EnsureStartup _sut;

    public EnsureStartupTests()
    {
        _sut = new EnsureStartup(_fileSystem, _appDataProvider);
        A.CallTo(() => _appDataProvider.AppDataPath).Returns(Path.Combine("C:", "my-dummy", "path"));
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<EnsureStartup> act1 = () => new EnsureStartup(_fileSystem, null!);
        Func<EnsureStartup> act2 = () => new EnsureStartup(null!, _appDataProvider);

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("RepositoryActionsV2.yaml")]
    [InlineData("RepoM.Filtering.yaml")]
    [InlineData("RepoM.Ordering.yaml")]
    [InlineData("appsettings.serilog.json")]
    public async Task EnsureFilesAsync_ShouldCheckIfFileExists(string filename)
    {
        // arrange
        var expectedFilename = Path.Combine("C:", "my-dummy", "path", filename);
        A.CallTo(() => _fileSystem.File.Exists(A<string?>._)).Returns(true);

        // act
        await _sut.EnsureFilesAsync();

        // assert
        A.CallTo(() => _fileSystem.File.Exists(expectedFilename)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("RepositoryActionsV2.yaml")]
    [InlineData("RepoM.Filtering.yaml")]
    [InlineData("RepoM.Ordering.yaml")]
    [InlineData("appsettings.serilog.json")]
    public async Task EnsureFilesAsync_ShouldThrowFileNotFoundException_WhenFileDoesNotExists(string filename)
    {
        // arrange
        A.CallTo(() => _fileSystem.File.Exists(A<string?>._)).Returns(true);
        A.CallTo(() => _fileSystem.File.Exists(A<string>.That.EndsWith(filename))).Returns(false);

        // act
        Func<Task> act = _sut.EnsureFilesAsync;

        // assert
        await act.Should().ThrowAsync<FileNotFoundException>().WithMessage("*" + filename);
    }

    [Theory]
    [InlineData("RepositoryActionsV2.yaml")]
    [InlineData("RepoM.Filtering.yaml")]
    [InlineData("RepoM.Ordering.yaml")]
    [InlineData("appsettings.serilog.json")]
    public async Task EnsureFilesAsync_ShouldCreate_WhenFileDoesNotExists(string filename)
    {
        // arrange
        A.CallTo(() => _fileSystem.File.Exists(A<string?>._)).Returns(true);
        A.CallTo(() => _fileSystem.File.Exists(A<string>.That.EndsWith(filename))).ReturnsNextFromSequence(false, true);

        // act
        await _sut.EnsureFilesAsync();

        // assert
        A.CallTo(() => _fileSystem.File.WriteAllBytesAsync(A<string>.That.EndsWith(filename), A<byte[]>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}