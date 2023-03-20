namespace RepoM.App.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RepoM.Api.Common;
using RepoM.Core.Plugin;

internal class ModuleService : IAsyncDisposable
{
    private readonly IAppSettingsService _appSettings;
    private readonly IModule[] _modules;
    private static Task? _stopTask;

    public ModuleService(IEnumerable<IModule> modules, IAppSettingsService appSettings)
    {
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        _modules = modules.ToArray();
    }

    public Task StartAsync()
    {
        return Task.WhenAll(_modules.Select(x => x.StartAsync()));
    }

    public Task StopAsync()
    {
        return Stop;
    }

    private Task Stop =>
        _stopTask ??= Task.Run(() => Task.WhenAll(_modules.Select(async module =>
            {
                await module.StopAsync().ConfigureAwait(false);

                if (module is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                }
                else if (module is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            })));

    public ValueTask DisposeAsync()
    {
        return new ValueTask(Stop);
    }
}