namespace RepoM.App.Tests.Plugins;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Api.Common;
using RepoM.App.Plugins;
using VerifyXunit;
using Xunit;

public class ModuleManagerTests
{
    private readonly IAppSettingsService _appSettingsService;

    public ModuleManagerTests()
    {
        _appSettingsService = A.Fake<IAppSettingsService>();
        A.CallTo(() => _appSettingsService.Plugins).Returns(new List<PluginSettings>
            {
                new ("Name1", "dll1", true),
                new ("Name2", "dll2", false),
            });
    }

    [Fact]
    public void Ctor_ShouldThrown_WhenArgumentIsNull()
    {
        // arrange

        // act
        var act1 = () => _ = new ModuleManager(_appSettingsService, null!);
        var act2 = () => _ = new ModuleManager(null!, NullLogger.Instance);

        // assert
        act1.Should().ThrowExactly<ArgumentNullException>();
        act2.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public async Task Ctor_ShouldInitializePluginsProperty()
    {
        // arrange
        var sut = new ModuleManager(_appSettingsService, NullLogger.Instance);

        // act
        var result = sut.Plugins.ToList();

        // assert
        await Verifier.Verify(result);
    }

    [Fact]
    public async Task SetEnabled_ShouldUpdateAppSettingsPluginList_WhenEnabledValueIsSame()
    {
        // arrange
        List<PluginSettings>? newlySetList = null;
        A.CallToSet(() => _appSettingsService.Plugins).Invokes(call => newlySetList = call.Arguments[0] as List<PluginSettings>);
        var sut = new ModuleManager(_appSettingsService, NullLogger.Instance);

        // act
        sut.Plugins[0].SetEnabled(true);

        // assert
        await Verifier.Verify(newlySetList);
    }

    [Fact]
    public async Task SetEnabled_ShouldUpdateAppSettingsPluginList_WhenEnabledValueHasChanged()
    {
        // arrange
        List<PluginSettings>? newlySetList = null;
        A.CallToSet(() => _appSettingsService.Plugins).Invokes(call => newlySetList = call.Arguments[0] as List<PluginSettings>);
        var sut = new ModuleManager(_appSettingsService, NullLogger.Instance);

        // act
        sut.Plugins[1].SetEnabled(true);

        // assert
        await Verifier.Verify(newlySetList);
    }

}