namespace RepoM.App.Tests;

using System;
using System.IO;
using System.IO.Abstractions;
using FakeItEasy;
using FluentAssertions;
using Meziantou.Xunit;
using Microsoft.Extensions.FileProviders;
using RepoM.Api.IO;
using RepoM.App.Services;
using Xunit;
using IFileInfo = Microsoft.Extensions.FileProviders.IFileInfo;

[DisableParallelization]    
public class ConfigBasedAppDataPathProviderFactoryTest
{
    private readonly IFileProvider _fileProvider = A.Dummy<IFileProvider>();
    private readonly IFileSystem _fileSystem = A.Dummy<IFileSystem>();

    [Fact]
    public void Create_ShouldSetPathToRelativeFolder_WhenAppSettingsJsonIsSet()
    {
        // arrange
        const string JSON =
            """
            // begin-snippet: appsettings_appsettings_path_relative lang:json
            {
              "App": {
                "AppSettingsPath": "MyConfigJson"
              }
            }
            // end-snippet
            """;
        A.CallTo(() => _fileProvider.GetFileInfo("appsettings.json")).Returns(CreateFileInfo(JSON));
        A.CallTo(() => _fileSystem.Path.GetFullPath(A<string>._))
         .ReturnsLazily(call => $"C:\\PathX\\{call.Arguments[0]}");
        var sut = new ConfigBasedAppDataPathProviderFactory([], _fileSystem, _fileProvider);

        // act
        AppDataPathProvider appDataPathProvider = sut.Create();

        appDataPathProvider.AppDataPath.Should().Be("C:\\PathX\\MyConfigJson");
        A.CallTo(() => _fileSystem.Path.GetFullPath("MyConfigJson")).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("--App:AppSettingsPath", "Arg_Relative_RepoMConfigFolder")]
    [InlineData("--App:AppSettingsPath=Arg_Relative_RepoMConfigFolder")]
    public void Create_ShouldSetPathToRelativeFolder_WhenArgumentsAreSet(params string[] args)
    {
        // arrange
        A.CallTo(() => _fileSystem.Path.GetFullPath("Arg_Relative_RepoMConfigFolder"))
         .Returns(Path.Combine("C:\\PathX\\Arg_Relative_RepoMConfigFolder"));
        var sut = new ConfigBasedAppDataPathProviderFactory(args, _fileSystem, _fileProvider);

        // act
        AppDataPathProvider appDataPathProvider = sut.Create();

        // assert
        appDataPathProvider.AppDataPath.Should().Be("C:\\PathX\\Arg_Relative_RepoMConfigFolder");
        A.CallTo(() => _fileSystem.Path.GetFullPath("Arg_Relative_RepoMConfigFolder")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Create_ShouldSetPathToArgumentValue_WhenBothArgumentAndEnvironmentVariableSet()
    {
        // arrange
        using IDisposable _ = SetEnvironmentVariable("REPOM_App__AppSettingsPath", "envVarX");
        A.CallTo(() => _fileSystem.Path.GetFullPath("Arg_Relative_RepoMConfigFolder"))
         .Returns(Path.Combine("C:\\PathX\\Arg_Relative_RepoMConfigFolder"));

        var sut = new ConfigBasedAppDataPathProviderFactory(["--App:AppSettingsPath=Arg_Relative_RepoMConfigFolder",], _fileSystem, _fileProvider);

        // act
        AppDataPathProvider appDataPathProvider = sut.Create();

        // assert
        appDataPathProvider.AppDataPath.Should().Be("C:\\PathX\\Arg_Relative_RepoMConfigFolder");
        A.CallTo(() => _fileSystem.Path.GetFullPath("Arg_Relative_RepoMConfigFolder")).MustHaveHappenedOnceExactly();
    }

    private static IFileInfo CreateFileInfo(string text)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);
        IFileInfo fi = A.Fake<IFileInfo>();
        A.CallTo(() => fi.Exists).Returns(true);
        A.CallTo(() => fi.Length).Returns(bytes.Length);
        A.CallTo(() => fi.PhysicalPath).Returns(null);
        A.CallTo(() => fi.CreateReadStream()).ReturnsLazily(() => new MemoryStream(bytes));
        return fi;
    }

    private static IDisposable SetEnvironmentVariable(string key, string value)
    {
        Environment.SetEnvironmentVariable(key, value);
        return new ClearEnvironmentVariable(key);
    }
}

file sealed class ClearEnvironmentVariable : IDisposable
{
    private readonly string _key;

    public ClearEnvironmentVariable(string key)
    {
        _key = key;
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(_key, null);
    }
}