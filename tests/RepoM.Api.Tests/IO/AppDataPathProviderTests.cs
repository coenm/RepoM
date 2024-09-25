namespace RepoM.Api.Tests.IO;

using System;
using System.IO.Abstractions;
using FakeItEasy;
using FluentAssertions;
using RepoM.Api.IO;
using Xunit;

public class AppDataPathProviderTests
{
    private readonly IFileSystem _fileSystem = A.Fake<IFileSystem>();

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<AppDataPathProvider> act1 = () => new AppDataPathProvider(CreateAppDataPathConfig(), null!);

        // assert
        act1.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Ctor_ShouldSetDefaultAppDataPath_WhenConfigHasNoValue(string? path)
    {
        // arrange
        A.CallTo(() => _fileSystem.Path.Combine(A<string>._, "RepoM")).Returns("Dummmy/RepoM");
        AppDataPathConfig config = CreateAppDataPathConfig(path);

        // act
        var sut = new AppDataPathProvider(config, _fileSystem);

        // assert
        sut.AppDataPath.Should().Be("Dummmy/RepoM");
        A.CallTo(() => _fileSystem.Path.Combine(A<string>._, "RepoM")).MustHaveHappenedOnceExactly();
        A.CallTo(_fileSystem.Path).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Ctor_ShouldSetAppDataPathToFullPath_WhenConfigIsSet()
    {
        // arrange
        A.CallTo(() => _fileSystem.Path.GetFullPath("Dummy42")).Returns("Dummy/RepoM42");
        AppDataPathConfig config = CreateAppDataPathConfig("Dummy42");

        // act
        var sut = new AppDataPathProvider(config, _fileSystem);

        // assert
        sut.AppDataPath.Should().Be("Dummy/RepoM42");
        A.CallTo(() => _fileSystem.Path.GetFullPath("Dummy42")).MustHaveHappenedOnceExactly();
        A.CallTo(_fileSystem.Path).MustHaveHappenedOnceExactly();
    }
    
    private static AppDataPathConfig CreateAppDataPathConfig(string? path = null)
    {
        return new AppDataPathConfig
        {
            AppSettingsPath = path,
        };
    }
}