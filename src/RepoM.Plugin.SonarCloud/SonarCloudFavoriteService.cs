namespace RepoM.Plugin.SonarCloud;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SonarQube.Net;
using SonarQube.Net.Common.Authentication;
using SonarQube.Net.Models;

internal class SonarCloudFavoriteService : ISonarCloudFavoriteService
{
    private readonly ISonarCloudConfiguration _appSettingsService;
    private SonarQubeClient? _client;
    private Task _task = Task.CompletedTask;
    private List<Favorite> _favorites = new(0);

    public SonarCloudFavoriteService(ISonarCloudConfiguration appSettingsService)
    {
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
    }

    public Task InitializeAsync()
    {
        var key = _appSettingsService.PersonalAccessToken;
        if (string.IsNullOrWhiteSpace(key))
        {
            return Task.CompletedTask;
        }
        
        _client = new SonarQubeClient(_appSettingsService.BaseUrl, new BasicAuthentication(key, string.Empty));

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