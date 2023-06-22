namespace RepoM.App.Plugins;

using System;
using Microsoft.Extensions.Logging;
using RepoM.Api.Common;

public class ModuleManager : IModuleManager
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly ILogger _logger;

    public event EventHandler? Changed;

    public ModuleManager(IAppSettingsService appSettingsService, ILogger logger)
    {
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var p = _appSettingsService.Plugins.ToArray();

        Changed?.Invoke(this, EventArgs.Empty);
    }
}