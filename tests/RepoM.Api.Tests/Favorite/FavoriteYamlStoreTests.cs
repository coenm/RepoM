namespace RepoM.Api.Tests.Favorite;

using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using FakeItEasy;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using RepoM.Api.Favorite;
using RepoM.Core.Plugin.Common;
using Xunit;

public class FavoriteYamlStoreTests
{
    private readonly IAppDataPathProvider _appDataPathProvider;
    private readonly MockFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly string _filePath;
    private readonly FavoriteYamlStore _sut;

    public FavoriteYamlStoreTests()
    {
        _appDataPathProvider = A.Fake<IAppDataPathProvider>();
        A.CallTo(() => _appDataPathProvider.AppDataPath).Returns("C:\\tmp-test");
        _fileSystem = new MockFileSystem();
        _logger = A.Fake<ILogger>();
        _filePath = "C:\\tmp-test\\favorites.yaml";
        _sut = new FavoriteYamlStore(_appDataPathProvider, _fileSystem, _logger);
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenAppDataPathProviderIsNull()
    {
        // act
        Action act = () => new FavoriteYamlStore(null!, _fileSystem, _logger);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenFileSystemIsNull()
    {
        // act
        Action act = () => new FavoriteYamlStore(_appDataPathProvider, null!, _logger);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenLoggerIsNull()
    {
        // act
        Action act = () => new FavoriteYamlStore(_appDataPathProvider, _fileSystem, null!);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Load_ShouldReturnEmptyList_WhenFileDoesNotExist()
    {
        // act
        IReadOnlyList<string> result = _sut.Load();

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Load_ShouldReturnEmptyList_WhenFileIsEmpty()
    {
        // arrange
        _fileSystem.AddFile(_filePath, new MockFileData(string.Empty));

        // act
        IReadOnlyList<string> result = _sut.Load();

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Load_ShouldReturnEmptyList_WhenFileContainsOnlyWhitespace()
    {
        // arrange
        _fileSystem.AddFile(_filePath, new MockFileData("   \n  "));

        // act
        IReadOnlyList<string> result = _sut.Load();

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Load_ShouldReturnFavorites_WhenFileContainsValidYaml()
    {
        // arrange
        _fileSystem.AddFile(_filePath, new MockFileData("- repo1\n- repo2\n- repo3\n"));

        // act
        IReadOnlyList<string> result = _sut.Load();

        // assert
        result.Should().BeEquivalentTo(["repo1", "repo2", "repo3"]);
    }

    [Fact]
    public void Load_ShouldReturnEmptyList_WhenYamlIsInvalid()
    {
        // arrange
        _fileSystem.AddFile(_filePath, new MockFileData("{{invalid yaml:::"));

        // act
        IReadOnlyList<string> result = _sut.Load();

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Save_ShouldCreateFile_WithSerializedYaml()
    {
        // arrange
        _fileSystem.AddDirectory("C:\\tmp-test");
        List<string> favorites = ["repo1", "repo2"];

        // act
        _sut.Save(favorites);

        // assert
        _fileSystem.File.Exists(_filePath).Should().BeTrue();
        var content = _fileSystem.File.ReadAllText(_filePath);
        content.Should().Contain("repo1");
        content.Should().Contain("repo2");
    }

    [Fact]
    public void Save_ShouldCreateDirectory_WhenItDoesNotExist()
    {
        // arrange
        List<string> favorites = ["repo1"];

        // act
        _sut.Save(favorites);

        // assert
        _fileSystem.Directory.Exists("C:\\tmp-test").Should().BeTrue();
        _fileSystem.File.Exists(_filePath).Should().BeTrue();
    }

    [Fact]
    public void Save_ShouldOverwriteExistingFile()
    {
        // arrange
        _fileSystem.AddFile(_filePath, new MockFileData("- old-repo\n"));

        // act
        _sut.Save(["new-repo"]);

        // assert
        var content = _fileSystem.File.ReadAllText(_filePath);
        content.Should().Contain("new-repo");
        content.Should().NotContain("old-repo");
    }

    [Fact]
    public void Save_ShouldWriteEmptySequence_WhenFavoritesIsEmpty()
    {
        // arrange
        _fileSystem.AddDirectory("C:\\tmp-test");

        // act
        _sut.Save([]);

        // assert
        _fileSystem.File.Exists(_filePath).Should().BeTrue();
    }

    [Fact]
    public void Load_ShouldReturnSavedFavorites_AfterRoundTrip()
    {
        // arrange
        _fileSystem.AddDirectory("C:\\tmp-test");
        List<string> favorites = ["alpha", "beta", "gamma"];

        // act
        _sut.Save(favorites);
        IReadOnlyList<string> result = _sut.Load();

        // assert
        result.Should().BeEquivalentTo(favorites);
    }
}
