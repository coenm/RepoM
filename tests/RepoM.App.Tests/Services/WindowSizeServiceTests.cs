namespace RepoM.App.Tests.Services;

using System;
using System.Windows;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RepoM.Api.Common;
using RepoM.App.Services;
using Xunit;

public class WindowSizeServiceTests
{
    [StaFact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<WindowSizeService> act1 = () => new WindowSizeService(new Window(), A.Dummy<IAppSettingsService>(), A.Dummy<IThreadDispatcher>(), null!);
        Func<WindowSizeService> act2 = () => new WindowSizeService(new Window(), A.Dummy<IAppSettingsService>(), null!, A.Dummy<ILogger>());
        Func<WindowSizeService> act3 = () => new WindowSizeService(new Window(), null!, A.Dummy<IThreadDispatcher>(), A.Dummy<ILogger>());
        Func<WindowSizeService> act4 = () => new WindowSizeService(null!, A.Dummy<IAppSettingsService>(), A.Dummy<IThreadDispatcher>(), A.Dummy<ILogger>());

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
        act3.Should().Throw<ArgumentNullException>();
        act4.Should().Throw<ArgumentNullException>();
    }

    [StaFact]
    public void Dispose_ShouldReturn_WhenNotRegistered()
    {
        // arrange
        var sut = new WindowSizeService(new Window(), A.Dummy<IAppSettingsService>(), A.Dummy<IThreadDispatcher>(), A.Dummy<ILogger>());

        // act
        Action act = () => sut.Dispose();

        // assert
        act.Should().NotThrow();
    }

    [StaFact]
    public void Unregister_ShouldReturn_WhenNotRegistered()
    {
        // arrange
        var sut = new WindowSizeService(new Window(), A.Dummy<IAppSettingsService>(), A.Dummy<IThreadDispatcher>(), A.Dummy<ILogger>());

        // act
        Action act = () => sut.Unregister();

        // assert
        act.Should().NotThrow();
    }
}


file class TestableWindowSizeService : WindowSizeService
{
    public TestableWindowSizeService(Window mainWindow, IAppSettingsService appSettings, IThreadDispatcher threadDispatcher, ILogger logger)
        : base(mainWindow, appSettings, threadDispatcher, logger)
    {
    }

    protected override string GetResolution()
    {
        return "dummy";
    }
}