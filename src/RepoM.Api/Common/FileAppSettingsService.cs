namespace RepoM.Api.Common;

using System;
using System.Collections.Generic;
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
    private readonly List<Action> _invalidationHandlers = new();
    private readonly IAppDataPathProvider _appDataPathProvider;

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

    private void Save()
    {
        var file = GetFileName();
        IDirectoryInfo? directoryInfo = _fileSystem.Directory.GetParent(file);
        if (directoryInfo == null)
        {
            _logger.LogError("Could not save configuration because no parent of '{file}' found", file);
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
            _logger.LogWarning(ex, "Could not check or create directory '{path}'. {message}", path, ex.Message);
        }

        try
        {
            _fileSystem.File.WriteAllText(GetFileName(), JsonConvert.SerializeObject(_settings, Formatting.Indented));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Could not save configuration to file '{file}'. {message}", file, ex.Message);
        }
    }

    private string GetFileName()
    {
        return Path.Combine(_appDataPathProvider.GetAppDataPath(), "appsettings.json");
    }

    private AppSettings Settings => _settings ??= Load();


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

    public double MenuWidth
    {
        get => Settings.MenuSize.Width;
        set
        {
            if (Math.Abs(value - Settings.MenuSize.Width) < 0.001)
            {
                return;
            }

            Settings.MenuSize.Width = value;

            NotifyChange();
            Save();
        }
    }

    public double MenuHeight
    {
        get => Settings.MenuSize.Height;
        set
        {
            if (Math.Abs(value - Settings.MenuSize.Height) < 0.001)
            {
                return;
            }

            Settings.MenuSize.Height = value;

            NotifyChange();
            Save();
        }
    }

    public List<string> ReposRootDirectories
    {
        get => Settings.ReposRootDirectories;
        set
        {
            Settings.ReposRootDirectories = value.ToList();

            NotifyChange();
            Save();
        }
    }

    public List<string> EnabledSearchProviders
    {
        get => Settings.EnabledSearchProviders;
        set
        {
            Settings.EnabledSearchProviders = value.ToList();

            NotifyChange();
            Save();
        }
    }

    public string SonarCloudPersonalAccessToken
    {
        get => Settings.SonarCloudPersonalAccessToken;
        set
        {
            if (value.Equals(Settings.SonarCloudPersonalAccessToken, StringComparison.InvariantCulture))
            {
                return;
            }

            Settings.SonarCloudPersonalAccessToken = value;

            NotifyChange();
            Save();
        }
    }

    public string AzureDevOpsPersonalAccessToken
    {
        get => Settings.AzureDevOps.PersonalAccessToken;
        set
        {
            if (value.Equals(Settings.AzureDevOps.PersonalAccessToken, StringComparison.InvariantCulture))
            {
                return;
            }

            Settings.AzureDevOps.PersonalAccessToken = value;

            NotifyChange();
            Save();
        }
    }

    public string AzureDevOpsBaseUrl
    {
        get => Settings.AzureDevOps.BaseUrl;
        set
        {
            if (value.Equals(Settings.AzureDevOps.BaseUrl, StringComparison.InvariantCulture))
            {
                return;
            }

            Settings.AzureDevOps.BaseUrl = value;

            NotifyChange();
            Save();
        }
    }

    public void RegisterInvalidationHandler(Action handler)
    {
        _invalidationHandlers.Add(handler);
    }

    public void NotifyChange()
    {
        _invalidationHandlers.ForEach(h => h.Invoke());
    }
}