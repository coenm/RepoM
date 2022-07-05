namespace RepoM.Plugin.SonarCloud.Tests;

using SonarQube.Net.Common.Authentication;
using SonarQube.Net;
using VerifyXunit;

[UsesVerify]
public class PocSonarCloud
{
    private readonly SonarQubeClient _client;
    private const string KEY = "Abc";

    public PocSonarCloud()
    {
        _client = new SonarQubeClient("https://sonarcloud.io", new BasicAuthentication("abc", string.Empty));
    }

    // [Fact]
    // public async Task GetFavorites()
    // {
    //     IEnumerable<Favorite>? result = await _client.SearchFavoritesAsync();
    //     
    //     // var x = await _client.AddFavoriteAsync("Acb");
    //     var x = await _client.RemoveFavoriteAsync("Acb");
    //
    //     result = await _client.SearchFavoritesAsync();
    //     await Verifier.Verify(new
    //         {
    //             result,
    //             x
    //         });
    //
    //     //_client.AddFavoriteAsync()
    // }
}