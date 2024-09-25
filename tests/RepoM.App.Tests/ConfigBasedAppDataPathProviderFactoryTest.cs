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
    private const string JSON =
        """
        // begin-snippet: appsettings_appsettings_path_relative
        {
          "App": {
            "AppSettingsPath": "MyConfigJson"
          }
        }
        // end-snippet
        """;

    private const string JSON_EMPTY =
        """
        {
          "App": null
        }
        """;

    private readonly IFileProvider _fileProvider = A.Dummy<IFileProvider>();
    private readonly IFileSystem _fileSystem = A.Dummy<IFileSystem>();

    public ConfigBasedAppDataPathProviderFactoryTest()
    {
        SetupGetFullPath();
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<ConfigBasedAppDataPathProviderFactory> act1 = () => new ConfigBasedAppDataPathProviderFactory([], null!);
        Func<ConfigBasedAppDataPathProviderFactory> act2 = () => new ConfigBasedAppDataPathProviderFactory(null!, A.Dummy<IFileSystem>());
        Func<ConfigBasedAppDataPathProviderFactory> act3 = () => new ConfigBasedAppDataPathProviderFactory([], A.Dummy<IFileSystem>(), null!);
        Func<ConfigBasedAppDataPathProviderFactory> act4 = () => new ConfigBasedAppDataPathProviderFactory([], null!, A.Dummy<IFileProvider>());
        Func<ConfigBasedAppDataPathProviderFactory> act5 = () => new ConfigBasedAppDataPathProviderFactory(null!, A.Dummy<IFileSystem>(), A.Dummy<IFileProvider>());

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
        act3.Should().Throw<ArgumentNullException>();
        act4.Should().Throw<ArgumentNullException>();
        act5.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_ShouldSetPathToRelativeFolder_WhenAppSettingsJsonIsSet()
    {
        // arrange
        AddFile("appsettings.json", JSON);
        var sut = new ConfigBasedAppDataPathProviderFactory([], _fileSystem, _fileProvider);

        // act
        AppDataPathProvider appDataPathProvider = sut.Create();

        appDataPathProvider.AppDataPath.Should().Be("C:\\PathX\\MyConfigJson");
        AssertGetFullPathCalled("MyConfigJson");
    }

    [Theory]
    [InlineData("--App:AppSettingsPath", "Arg_Relative_RepoMConfigFolder")]
    [InlineData("--App:AppSettingsPath=Arg_Relative_RepoMConfigFolder")]
    [InlineData("abc", "--App:AppSettingsPath", "Arg_Relative_RepoMConfigFolder")]
    [InlineData("--App:AppSettingsPath", "Will_be_overridden", "--App:AppSettingsPath", "Arg_Relative_RepoMConfigFolder")]
    public void Create_ShouldSetPathToRelativeFolder_WhenArgumentsAreSet(params string[] args)
    {
        // arrange
        var sut = new ConfigBasedAppDataPathProviderFactory(args, _fileSystem, _fileProvider);

        // act
        AppDataPathProvider appDataPathProvider = sut.Create();

        // assert
        appDataPathProvider.AppDataPath.Should().Be("C:\\PathX\\Arg_Relative_RepoMConfigFolder");
        AssertGetFullPathCalled("Arg_Relative_RepoMConfigFolder");
    }

    [Fact]
    public void Create_ShouldSetPathToArgumentValue_WhenBothArgumentAndEnvironmentVariableSet()
    {
        // arrange
        using IDisposable _ = SetEnvironmentVariable("REPOM_App__AppSettingsPath", "envVarX");
        var sut = new ConfigBasedAppDataPathProviderFactory(["--App:AppSettingsPath=Arg_Relative_RepoMConfigFolder",], _fileSystem, _fileProvider);

        // act
        AppDataPathProvider appDataPathProvider = sut.Create();

        // assert
        appDataPathProvider.AppDataPath.Should().Be("C:\\PathX\\Arg_Relative_RepoMConfigFolder");
        AssertGetFullPathCalled();
    }

    [Fact]
    public void Create_ArgumentsPriority1()
    {
        // arrange
        AddFile("appsettings.json", JSON);
        using IDisposable _ = SetEnvironmentVariable("REPOM_App__AppSettingsPath", "envVarX");
        var sut = new ConfigBasedAppDataPathProviderFactory(["--App:AppSettingsPath=Arg_Relative_RepoMConfigFolder",], _fileSystem, _fileProvider);

        // act
        AppDataPathProvider appDataPathProvider = sut.Create();

        // assert
        appDataPathProvider.AppDataPath.Should().Be("C:\\PathX\\Arg_Relative_RepoMConfigFolder");
        AssertGetFullPathCalled();
    }

    [Fact]
    public void Create_AppSettingsPriority2()
    {
        // arrange
        AddFile("appsettings.json", JSON);
        using IDisposable _ = SetEnvironmentVariable("REPOM_App__AppSettingsPath", "envVarX");
        var sut = new ConfigBasedAppDataPathProviderFactory([], _fileSystem, _fileProvider);

        // act
        AppDataPathProvider appDataPathProvider = sut.Create();

        // assert
        appDataPathProvider.AppDataPath.Should().Be("C:\\PathX\\MyConfigJson");
        AssertGetFullPathCalled();
    }

    [Fact]
    public void Create_EnvironmentPriority3()
    {
        // arrange
        AddFile("appsettings.json", JSON_EMPTY);
        using IDisposable _ = SetEnvironmentVariable("REPOM_App__AppSettingsPath", "envVarX");
        var sut = new ConfigBasedAppDataPathProviderFactory([], _fileSystem, _fileProvider);

        // act
        AppDataPathProvider appDataPathProvider = sut.Create();

        // assert
        appDataPathProvider.AppDataPath.Should().Be("C:\\PathX\\envVarX");
        AssertGetFullPathCalled();
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

    private void AddFile(string filename, string content)
    {
        A.CallTo(() => _fileProvider.GetFileInfo(filename))
         .Returns(CreateFileInfo(content));
    }

    private void SetupGetFullPath()
    {
        A.CallTo(() => _fileSystem.Path.GetFullPath(A<string>._))
         .ReturnsLazily(call => $"C:\\PathX\\{call.Arguments[0]}");
    }

    private void AssertGetFullPathCalled(string? file = null)
    {
        if (file is null)
        {
            A.CallTo(() => _fileSystem.Path.GetFullPath(A<string>._)).MustHaveHappenedOnceExactly();
        }
        else
        {
            A.CallTo(() => _fileSystem.Path.GetFullPath(file)).MustHaveHappenedOnceExactly();
        }
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