namespace RepoM.Api.Common;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Newtonsoft.Json;
using RepoM.Api.Git.AutoFetch;
using RepoM.Api.IO;

public class FileAppSettingsService : IAppSettingsService
{
    private readonly IFileSystem _fileSystem;
    private AppSettings? _settings;
    private readonly List<Action> _invalidationHandlers = new();
    private readonly IAppDataPathProvider _appDataPathProvider;

    public FileAppSettingsService(IAppDataPathProvider appDataPathProvider, IFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
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
        var path = _fileSystem.Directory.GetParent(file).FullName;

        if (!_fileSystem.Directory.Exists(path))
        {
            _fileSystem.Directory.CreateDirectory(path);
        }

        try
        {
            _fileSystem.File.WriteAllText(GetFileName(), JsonConvert.SerializeObject(_settings, Formatting.Indented));
        }
        catch
        {
            /* Our app settings are not critical. For our purposes, we want to ignore IO exceptions */
        }
    }

    private string GetFileName()
    {
        return Path.Combine(_appDataPathProvider.GetAppDataPath(), "appsettings.json");
    }

    private AppSettings Settings => _settings ??= Load();

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