namespace RepoM.App.Plugins;

using System;
using System.Collections.Generic;
using System.Linq;
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

        Plugins = _appSettingsService.Plugins
                                     .Select(x => new PluginModel(this)
                                         {
                                             Description = "",
                                             Dll = x.DllName,
                                             Enabled = x.Enabled,
                                             Found = false,
                                         })
                                     .ToList();

        Changed?.Invoke(this, EventArgs.Empty);
    }

    public IReadOnlyList<PluginModel> Plugins { get; }
}

public sealed class PluginModel
{
    private readonly IModuleManager _moduleManager;

    public PluginModel(IModuleManager moduleManager)
    {
        _moduleManager = moduleManager ?? throw new ArgumentNullException(nameof(moduleManager));
    }

    public string Name { get; init; } = null!;

    public string Dll { get; init; } = null!;
    
    public bool Enabled { get; set; } = true;

    public bool Found { get; set; }

    public string? Description { get; set; }
}