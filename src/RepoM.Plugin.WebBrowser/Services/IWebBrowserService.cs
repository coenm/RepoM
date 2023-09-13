namespace RepoM.Plugin.WebBrowser.Services;

internal interface IWebBrowserService
{
    void OpenUrl(string url);

    void OpenUrl(string url, string profile);
}