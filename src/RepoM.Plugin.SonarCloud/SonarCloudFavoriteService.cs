namespace RepoM.Plugin.SonarCloud;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RepoM.Api.Common;
using SonarQube.Net;
using SonarQube.Net.Common.Authentication;
using SonarQube.Net.Models;

internal class SonarCloudFavoriteService : ISonarCloudFavoriteService
{
    const string SONAR_CLOUD_URL = "https://sonarcloud.io";

    private readonly IAppSettingsService _appSettingsService;
    private SonarQubeClient? _client;
    private Task _task = Task.CompletedTask;
    private List<Favorite> _favorites = new(0);

    public SonarCloudFavoriteService(IAppSettingsService appSettingsService)
    {
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
    }

    public Task InitializeAsync()
    {
        var key = _appSettingsService.SonarCloudPersonalAccessToken;
        if (string.IsNullOrWhiteSpace(key))
        {
            return Task.CompletedTask;
        }
        
        _client = new SonarQubeClient(SONAR_CLOUD_URL, new BasicAuthentication(key, string.Empty));

        _task = Task.Run(async () =>
            {
                IEnumerable<Favorite>? result = await _client.SearchFavoritesAsync();
                if (result != null)
                {
                    _favorites = result.ToList();
                }
            });

        return Task.CompletedTask;
    }

    public bool IsInitialized => _client != null;

    public async Task SetFavorite(string repoKey)
    {
        SonarQubeClient? client = _client;
        if (client == null)
        {
            return;
        }

        try
        {
            _= await client.AddFavoriteAsync(repoKey);
            if (_task.IsCompleted)
            {
                _task = Task.Run(async () =>
                    {
                        IEnumerable<Favorite>? result = await client.SearchFavoritesAsync();
                        if (result != null)
                        {
                            _favorites = result.ToList();
                        }
                    });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public bool IsFavorite(string repoKey)
    {
        // qualifiers??
        return _favorites.Exists(x => x.Key == repoKey);
    }
}