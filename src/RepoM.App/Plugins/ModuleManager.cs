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

    public ModuleManager(IAppSettingsService appSettingsService, ILogger logger)
    {
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Plugins = _appSettingsService.Plugins
            .Select(plugin => new PluginModel(plugin.Enabled, (key, enabled) => Update(key, enabled))
            {
                Name = plugin.Name,
                Dll = plugin.DllName,
                Found = true,
            })
            .ToList();
    }

    public IReadOnlyList<PluginModel> Plugins { get; }

    // this happends in UI thread.
    private void Update(string dll, bool enabled)
    {
        _logger.LogInformation(enabled ? $"Enabling plugin {dll}" : $"Disabling plugin {dll}");
        var pluginsCopy = _appSettingsService.Plugins.ToList();
        PluginSettings? item = pluginsCopy.SingleOrDefault(plugin => plugin.DllName == dll);
        if (item == null)
        {
            _logger.LogError("Could not find plugin {dll}", dll);
            return;
        }

        item.Enabled = enabled;
        _appSettingsService.Plugins = pluginsCopy; // this triggers the save
    }
}