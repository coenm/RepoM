namespace RepoM.App.Tests.Services;

using System;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RepoM.App.Services;
using RepoM.Core.Plugin.Common;
using Xunit;

public class FileBasedPackageConfigurationTest
{
    private readonly IAppDataPathProvider _appDataPathProvider;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly string _filename;

    public FileBasedPackageConfigurationTest()
    {
        _appDataPathProvider = A.Fake<IAppDataPathProvider>();
        _fileSystem = A.Fake<IFileSystem>();
        _logger = A.Fake<ILogger>();
        _filename = "dummy";

        A.CallTo(() => _appDataPathProvider.GetAppDataPath()).Returns("C:\\tmp-test\\");
        A.CallTo(() => _fileSystem.File.Exists("C:\\tmp-test\\Module\\dummy.json")).Returns(true);
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<FileBasedPackageConfiguration> act1 = () => new FileBasedPackageConfiguration(_appDataPathProvider, _fileSystem, _logger, null!);
        Func<FileBasedPackageConfiguration> act2 = () => new FileBasedPackageConfiguration(_appDataPathProvider, _fileSystem, null!, _filename);
        Func<FileBasedPackageConfiguration> act3 = () => new FileBasedPackageConfiguration(_appDataPathProvider, null!, _logger, _filename);
        Func<FileBasedPackageConfiguration> act4 = () => new FileBasedPackageConfiguration(null!, _fileSystem, _logger, _filename);

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
        act3.Should().Throw<ArgumentNullException>();
        act4.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetConfigurationVersionAsync_ShouldReturnNull_WhenFileNotFound()
    {
        // arrange

        A.CallTo(() => _fileSystem.File.Exists("C:\\tmp-test\\Module\\dummy.json")).Returns(false);
        var sut = new FileBasedPackageConfiguration(_appDataPathProvider, _fileSystem, _logger, _filename);

        // act
        var result = await sut.GetConfigurationVersionAsync();
        
        // assert
        result.Should().BeNull();
        A.CallTo(() => _fileSystem.File.Exists("C:\\tmp-test\\Module\\dummy.json")).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("invalid json")]
    [InlineData("{  }")]
    [InlineData("{ VersioN = 34 }")]
    public async Task GetConfigurationVersionAsync_ShouldReturnNull_WhenFileDoesNotContainJson(string fileContent)
    {
        // arrange

        A.CallTo(() => _fileSystem.File.Exists("C:\\tmp-test\\Module\\dummy.json")).Returns(true);
        A.CallTo(() => _fileSystem.File.ReadAllTextAsync("C:\\tmp-test\\Module\\dummy.json", A<CancellationToken>._)).ReturnsLazily(() => fileContent);
        var sut = new FileBasedPackageConfiguration(_appDataPathProvider, _fileSystem, _logger, _filename);

        // act
        var result = await sut.GetConfigurationVersionAsync();
        
        // assert
        result.Should().BeNull();
        A.CallTo(() => _fileSystem.File.Exists("C:\\tmp-test\\Module\\dummy.json")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fileSystem.File.ReadAllTextAsync("C:\\tmp-test\\Module\\dummy.json", A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}