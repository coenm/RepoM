namespace RepoM.Api.Tests;

using FakeItEasy;
using RepoM.Api.Plugins;
using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SimpleInjector;
using Xunit;

public class BootstrapperTests
{
    private readonly CoreBootstrapper _sut;
    private readonly IPluginFinder _pluginFinder;
    private readonly IFileSystem _fileSystem;
    private readonly ILoggerFactory _loggerFactory;

    private const string BASE_DIRECTORY = @"C:\dir\";

    public BootstrapperTests()
    {
        _pluginFinder = A.Fake<IPluginFinder>();
        _fileSystem = A.Fake<IFileSystem>();
        _loggerFactory = A.Fake<ILoggerFactory>();
        _sut = new CoreBootstrapper(_pluginFinder, _fileSystem, _loggerFactory);
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<CoreBootstrapper> act1 = () => new CoreBootstrapper(A.Dummy<IPluginFinder>(), A.Dummy<IFileSystem>(), null!);
        Func<CoreBootstrapper> act2 = () => new CoreBootstrapper(A.Dummy<IPluginFinder>(), null!, A.Dummy<ILoggerFactory>());
        Func<CoreBootstrapper> act3 = () => new CoreBootstrapper(null!, A.Dummy<IFileSystem>(), A.Dummy<ILoggerFactory>());

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
        act3.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task LoadAndRegisterPluginsAsync_ShouldThrow_WhenContainerIsNull()
    {
        // arrange
        Container container = null!;

        // act
        Func<Task> act = async () => await _sut.LoadAndRegisterPluginsAsync(container, BASE_DIRECTORY);

        // assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}