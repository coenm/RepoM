namespace RepoM.StartupPlugin.Demo.Tests.TestFramework;

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using RepoM.Tests.TestFramework;
using Xunit;

// FluentWPF caches thread-bound chrome, so all demo scenarios share one WPF dispatcher.
[CollectionDefinition("Demo WPF scenarios", DisableParallelization = true)]
public sealed class DemoWpfStaCollection : ICollectionFixture<DemoWpfStaFixture>
{
    public const string Name = "Demo WPF scenarios";
}

public sealed class DemoWpfStaFixture : IAsyncLifetime
{
    private readonly TaskCompletionSource<Dispatcher> _dispatcherReady =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _dispatcherStopped =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    private IDisposable? _desktop;

    public Task InitializeAsync()
    {
        var thread = new Thread(() =>
        {
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            var application = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            _dispatcherReady.SetResult(dispatcher);
            application.Run();
            _dispatcherStopped.SetResult();
        })
        {
            IsBackground = true,
            Name = "RepoM demo WPF test dispatcher",
        };
        thread.SetApartmentState(ApartmentState.STA);
        _desktop = WpfDesktopAttribute.Acquire();
        try
        {
            thread.Start();
        }
        catch
        {
            _desktop.Dispose();
            _desktop = null;
            throw;
        }

        return _dispatcherReady.Task;
    }

    public async Task RunAsync(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        Dispatcher dispatcher = await _dispatcherReady.Task;
        await dispatcher.InvokeAsync(action).Task.Unwrap();
    }

    public async Task DisposeAsync()
    {
        if (_desktop == null)
        {
            return;
        }

        try
        {
            Dispatcher dispatcher = await _dispatcherReady.Task;
            // Let pending popup work finish before shutting down the application.
            await dispatcher.InvokeAsync(() => Application.Current.Shutdown(), DispatcherPriority.ApplicationIdle)
                .Task.WaitAsync(TimeSpan.FromSeconds(10));
            await _dispatcherStopped.Task.WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally
        {
            _desktop.Dispose();
        }
    }
}
