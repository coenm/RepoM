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

public class WindowSizeServiceTests : IDisposable
{
    private readonly IThreadDispatcher _threadDispatcher;
    private readonly IAppSettingsService _appSettingsService;
    private Window? _window;

    public WindowSizeServiceTests()
    {
        _threadDispatcher = A.Fake<IThreadDispatcher>();
        SynchronizationContext current = SynchronizationContext.Current!;
        A.CallTo(() => _threadDispatcher.SynchronizationContext).Returns(current);

        _appSettingsService = A.Fake<IAppSettingsService>();
    }

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

    [WpfFact]
    public void Register_ShouldReturn()
    {
        // arrange
        var sut = new TestableWindowSizeService(new Window(), A.Dummy<IAppSettingsService>(), _threadDispatcher, A.Dummy<ILogger>());

        // act
        Action act = () => sut.Register();

        // assert
        act.Should().NotThrow();
    }

    [WpfFact]
    public void UnRegister_ShouldReturn_WhenRegistered()
    {
        // arrange
        var sut = new TestableWindowSizeService(new Window(), A.Dummy<IAppSettingsService>(), _threadDispatcher, A.Dummy<ILogger>());
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
        List<int> callingThreadIds = [];
        var currentThreadId = Environment.CurrentManagedThreadId;
        var signal = new ManualResetEventSlim();

        _window = new Window { Width = 1, Height = 1, };
        
        MenuSize? configuredMenuSize = new MenuSize
            {
                MenuHeight = 12,
                MenuWidth = 34,
            };
        A.CallTo(() => _appSettingsService.TryGetMenuSize("1x2", out configuredMenuSize)).Returns(true);

        List<MenuSize> updatedMenuSize = [];
        _ = A.CallTo(() => _appSettingsService.UpdateMenuSize("1x2", A<MenuSize>._))
         .Invokes(call =>
             {
                 callingThreadIds.Add(Environment.CurrentManagedThreadId);
                 updatedMenuSize.Add((MenuSize)call.Arguments[1]!);
                 signal.Set();
             });

        var sut = new TestableWindowSizeService(_window, _appSettingsService, _threadDispatcher, A.Dummy<ILogger>());
        sut.Register();

        // act
        ResizeWindow(_window, 40, 140);
        sut.WaitForWindowSizeChanged(signal);

        // assert
        updatedMenuSize.Single().Should().BeEquivalentTo(
            new MenuSize
            {
                MenuHeight = 40,
                MenuWidth = 140,
            });
        callingThreadIds.Should().HaveCountGreaterOrEqualTo(1);
        callingThreadIds.Should().NotContain(currentThreadId);

    }

    void IDisposable.Dispose()
    {
        try
        {
            _window?.Close();
        }
        catch (Exception)
        {
            // ignore
        }
    }

    private static void ResizeWindow(Window window, double height, double width)
    {
        window.Show();
        window.Height = height;
        window.Width = width;
        window.Hide();
    }
}

file class TestableWindowSizeService : WindowSizeService
{
    public TestableWindowSizeService(Window mainWindow, IAppSettingsService appSettings, IThreadDispatcher threadDispatcher, ILogger logger)
        : base(mainWindow, appSettings, threadDispatcher, logger)
    {
    }

    public bool WaitForWindowSizeChanged(ManualResetEventSlim signal)
    {
        return signal.Wait(ThrottleWindowSizeChanged.Add(TimeSpan.FromSeconds(5)));
    }

    protected override string GetResolution()
    {
        return "1x2";
    }
}