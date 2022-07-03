namespace RepoM.Plugin.SonarCloud;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RepoM.Api.Common.Common;
using SonarQube.Net;
using SonarQube.Net.Common.Authentication;
using SonarQube.Net.Models;

public class SonarCloudFavoriteService
{
    private readonly IAppSettingsService _appSettingsService;
    private SonarQubeClient? _client;
    private Task _task = Task.CompletedTask;
    private List<Favorite> _favorites = new(0);

    public SonarCloudFavoriteService(IAppSettingsService appSettingsService)
    {
        _appSettingsService = appSettingsService;
    }

    public Task InitializeAsync()
    {
        var key = _appSettingsService.SonarCloudPersonalAccessToken;
        if (string.IsNullOrWhiteSpace(key))
        {
            return Task.CompletedTask;
        }
        _client = new SonarQubeClient(
            "https://sonarcloud.io",
            new BasicAuthentication(key, string.Empty));

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

    public async Task SetFavorite(string repoKey)
    {
        SonarQubeClient? c = _client;
        if (c == null)
        {
            return;
        }

        try
        {
            _= await c.AddFavoriteAsync(repoKey);
            if (_task.IsCompleted)
            {
                _task = Task.Run(async () =>
                    {
                        IEnumerable<Favorite>? result = await c.SearchFavoritesAsync();
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
        return _favorites.Any(x => x.Key == repoKey);
    }
}