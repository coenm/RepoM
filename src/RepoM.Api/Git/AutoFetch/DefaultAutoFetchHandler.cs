namespace RepoM.Api.Git.AutoFetch;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RepoM.Api.Common;
using RepoM.Core.Repositories.Adapters;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Store;

public class DefaultAutoFetchHandler : IAutoFetchHandler
{
    private bool _active;
    private AutoFetchMode? _mode;
    private readonly Timer _timer;
    private readonly Dictionary<AutoFetchMode, AutoFetchProfile> _profiles;
    private int _lastFetchRepository = -1;
    private readonly IAppSettingsService _appSettingsService;
    private readonly IRepositoryStore _repositoryStore;
    private readonly IRepositoryWriter _repositoryWriter;

    public DefaultAutoFetchHandler(
        IAppSettingsService appSettingsService,
        IRepositoryStore repositoryStore,
        IRepositoryWriter repositoryWriter)
    {
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        _repositoryStore = repositoryStore ?? throw new ArgumentNullException(nameof(repositoryStore));
        _repositoryWriter = repositoryWriter ?? throw new ArgumentNullException(nameof(repositoryWriter));
        _appSettingsService.RegisterInvalidationHandler(() => Mode = _appSettingsService.AutoFetchMode);

        _profiles = new Dictionary<AutoFetchMode, AutoFetchProfile>
            {
                { AutoFetchMode.Off, new AutoFetchProfile { PauseBetweenFetches = TimeSpan.MaxValue, } },
                { AutoFetchMode.Discretely, new AutoFetchProfile { PauseBetweenFetches = TimeSpan.FromMinutes(5), } },
                { AutoFetchMode.Adequate, new AutoFetchProfile { PauseBetweenFetches = TimeSpan.FromMinutes(1), } },
                { AutoFetchMode.Aggressive, new AutoFetchProfile { PauseBetweenFetches = TimeSpan.FromSeconds(2), } },
            };

        _timer = new Timer(FetchNext, null, Timeout.Infinite, Timeout.Infinite);
    }

    private void UpdateBehavior()
    {
        if (!_mode.HasValue)
        {
            return;
        }

        UpdateBehavior(_mode.Value);
    }

    private void UpdateBehavior(AutoFetchMode mode)
    {
        AutoFetchProfile profile = _profiles[mode];

        var milliseconds = (int)profile.PauseBetweenFetches.TotalMilliseconds;
        if (profile.PauseBetweenFetches == TimeSpan.MaxValue)
        {
            milliseconds = Timeout.Infinite;
        }

        _timer.Change(milliseconds, Timeout.Infinite);
    }

    private void FetchNext(object? timerState)
    {
        IReadOnlyCollection<RepositoryInfo> items = _repositoryStore.Items;
        if (items.Count == 0)
        {
            return;
        }

        var repositories = items
            .OrderBy(r => r.Name)
            .ToArray();

        // temporarily disable the timer to prevent parallel fetch executions
        UpdateBehavior(AutoFetchMode.Off);

        _lastFetchRepository++;

        if (repositories.Length <= _lastFetchRepository)
        {
            _lastFetchRepository = 0;
        }

        RepositoryInfo repoInfo = repositories[_lastFetchRepository];
        var adapter = new RepositoryInfoAdapter(repoInfo);

        try
        {
            _repositoryWriter.Fetch(adapter);
        }
        catch
        {
            // nothing to see here
        }
        finally
        {
            // re-enable the timer to get to the next fetch
            UpdateBehavior();
        }
    }

    public bool Active
    {
        get => _active;
        set
        {
            _active = value;

            if (value && _mode == null)
            {
                Mode = _appSettingsService.AutoFetchMode;
            }

            UpdateBehavior();
        }
    }

    public AutoFetchMode Mode
    {
        get => _mode ?? AutoFetchMode.Off;
        set
        {
            if (value == _mode)
            {
                return;
            }

            _mode = value;
            UpdateBehavior();
        }
    }
}
