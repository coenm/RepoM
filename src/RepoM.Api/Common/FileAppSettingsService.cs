namespace RepoM.Api.Common;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RepoM.Api.Git.AutoFetch;
using RepoM.Core.Plugin.Common;

public class FileAppSettingsService : IAppSettingsService
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private AppSettings? _settings;
    private readonly List<Action> _invalidationHandlers = new(1);
    private readonly IAppDataPathProvider _appDataPathProvider;

    private List<PluginSettings>? _plugins;
    private static readonly JsonSerializerSettings _jsonSerializationSettings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

    public FileAppSettingsService(IAppDataPathProvider appDataPathProvider, IFileSystem fileSystem, ILogger logger)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _appDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
    }

    private AppSettings Load()
    {
        var file = GetFileName();

        if (!_fileSystem.File.Exists(file))
        {
            return AppSettings.Default;
        }

        try
        {
            var json = _fileSystem.File.ReadAllText(file);
            AppSettings? result = JsonConvert.DeserializeObject<AppSettings>(json);
            return result ?? AppSettings.Default;
        }
        catch
        {
            /* Our app settings are not critical. For our purposes, we want to ignore IO exceptions */
        }

        return AppSettings.Default;
    }
    
    public string SortKey
    {
        get => Settings.SortKey;
        set
        {
            if (value == Settings.SortKey)
            {
                return;
            }

            Settings.SortKey = value;

            NotifyChange();
            Save();
        }
    }

    public string QueryParserKey
    {
        get => Settings.SelectedQueryParser;
        set
        {
            if (value == Settings.SelectedQueryParser)
            {
                return;
            }

            Settings.SelectedQueryParser = value;

            NotifyChange();
            Save();
        }
    }

    public string SelectedFilter
    {
        get => Settings.SelectedFilter;
        set
        {
            if (value == Settings.SelectedFilter)
            {
                return;
            }

            Settings.SelectedFilter = value;

            NotifyChange();
            Save();
        }
    }
    
    public AutoFetchMode AutoFetchMode
    {
        get => Settings.AutoFetchMode;
        set
        {
            if (value == Settings.AutoFetchMode)
            {
                return;
            }

            Settings.AutoFetchMode = value;

            NotifyChange();
            Save();
        }
    }

    public bool PruneOnFetch
    {
        get => Settings.PruneOnFetch;
        set
        {
            if (value == Settings.PruneOnFetch)
            {
                return;
            }

            Settings.PruneOnFetch = value;

            NotifyChange();
            Save();
        }
    }

    public void UpdateMenuSize(string resolution, MenuSize size)
    {
        Settings.PreferredMenuSizes[resolution] = new Size
            {
                Height = size.MenuHeight,
                Width = size.MenuWidth,
            };

        NotifyChange();
        Save();
    }

    public bool TryGetMenuSize(string resolution, [NotNullWhen(true)] out MenuSize? size)
    {
        if (Settings.PreferredMenuSizes.TryGetValue(resolution, out Size? value))
        {
            size = new MenuSize
                {
                    MenuHeight = value.Height,
                    MenuWidth = value.Width,
                };
        }
        else
        {
            size = null;
        }

        return size != null;
    }

    public List<string> ReposRootDirectories
    {
        get => Settings.ReposRootDirectories;
        set
        {
            Settings.ReposRootDirectories = [.. value, ];

            NotifyChange();
            Save();
        }
    }

    public List<PluginSettings> Plugins
    {
        get => _plugins ??= Convert(Settings.Plugins);
        set
        {
            _plugins = value;
            Settings.Plugins = value
               .Select(x => new PluginOptions
               {
                   DllName = x.DllName,
                   Name = x.Name,
                   Enabled = x.Enabled,
               })
               .ToList();

            NotifyChange();
            Save();
        }
    }

    private AppSettings Settings => _settings ??= Load();
    
    private void Save()
    {
        var file = GetFileName();
        IDirectoryInfo? directoryInfo = _fileSystem.Directory.GetParent(file);
        if (directoryInfo == null)
        {
            _logger.LogError("Could not save configuration because no parent of '{File}' found", file);
            return;
        }

        var path = directoryInfo.FullName;

        try
        {
            if (!_fileSystem.Directory.Exists(path))
            {
                _fileSystem.Directory.CreateDirectory(path);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not check or create directory '{Path}'. {Message}", path, ex.Message);
        }

        try
        {
            var jsonString = JsonConvert.SerializeObject(_settings, _jsonSerializationSettings);
            _fileSystem.File.WriteAllText(file, jsonString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not save configuration to file '{File}'. {Message}", file, ex.Message);
        }
    }

    private string GetFileName()
    {
        return Path.Combine(_appDataPathProvider.AppDataPath, "appsettings.json");
    }
    
    private static List<PluginSettings> Convert(IEnumerable<PluginOptions> plugins)
    {
        return plugins.Select(pluginOptions => new PluginSettings(pluginOptions.Name, pluginOptions.DllName, pluginOptions.Enabled)).ToList();
    }

    public void RegisterInvalidationHandler(Action handler)
    {
        _invalidationHandlers.Add(handler);
    }

    private void NotifyChange()
    {
       _invalidationHandlers.ForEach(h => h.Invoke());
    }
}