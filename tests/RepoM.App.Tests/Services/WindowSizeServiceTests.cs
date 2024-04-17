namespace RepoM.App.Tests.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    }

    [WpfFact]
    public void Register_ShouldReturn()
    {
        // arrange
        IThreadDispatcher threadDispatcher = A.Fake<IThreadDispatcher>();
        SynchronizationContext current = SynchronizationContext.Current!;
        A.CallTo(() => threadDispatcher.SynchronizationContext).Returns(current);
        var sut = new TestableWindowSizeService(new Window(), A.Dummy<IAppSettingsService>(), threadDispatcher, A.Dummy<ILogger>());

        // act
        Action act = () => sut.Register();

        // assert
        act.Should().NotThrow();
    }

    [WpfFact]
    public void UnRegister_ShouldReturn_WhenRegistered()
    {
        // arrange
        IThreadDispatcher threadDispatcher = A.Fake<IThreadDispatcher>();
        SynchronizationContext current = SynchronizationContext.Current!;
        A.CallTo(() => threadDispatcher.SynchronizationContext).Returns(current);
        var sut = new TestableWindowSizeService(new Window(), A.Dummy<IAppSettingsService>(), threadDispatcher, A.Dummy<ILogger>());
        sut.Register();

        // act
        Action act = () => sut.Unregister();

        // assert
        act.Should().NotThrow();
    }

    [WpfFact]
    public void Register_ShouldSubscribeAndHandleSizeChangedEvents()
    {
        // arrange
        IThreadDispatcher threadDispatcher = A.Fake<IThreadDispatcher>();
        SynchronizationContext current = SynchronizationContext.Current!;
        A.CallTo(() => threadDispatcher.SynchronizationContext).Returns(current);
        var window = new Window { Width = 1, Height = 1, };
        IAppSettingsService appSettingsService = A.Dummy<IAppSettingsService>();

        List<int> originalThread = [];
        MenuSize? configuredMenuSize = new MenuSize
            {
                MenuHeight = 123,
                MenuWidth = 345,
            };
        A.CallTo(() => appSettingsService.TryGetMenuSize("1x2", out configuredMenuSize))
         .Invokes(_ => originalThread.Add(Environment.CurrentManagedThreadId))
         .Returns(true);

        var mre = new ManualResetEventSlim();
        List<MenuSize> updatedMenuSize = [];
        _ = A.CallTo(() => appSettingsService.UpdateMenuSize("1x2", A<MenuSize>._))
         .Invokes(call =>
             {
                 originalThread.Add(Environment.CurrentManagedThreadId);
                 updatedMenuSize.Add((MenuSize)call.Arguments[1]!);
                 mre.Set();
             });

        var sut = new TestableWindowSizeService(window, appSettingsService, threadDispatcher, A.Dummy<ILogger>());
        sut.Register();

        // act
        window.Show();
        window.Height += 10;
        window.Width += 10;
        mre.Wait(TimeSpan.FromSeconds(10));

        // assert
        updatedMenuSize.Single().Should().BeEquivalentTo(
            new MenuSize
            {
                MenuHeight = 133,
                MenuWidth = 355,
            });
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
        return "1x2";
    }
}