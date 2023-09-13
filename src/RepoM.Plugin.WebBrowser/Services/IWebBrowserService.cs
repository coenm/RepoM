namespace RepoM.Plugin.WebBrowser.Services;

internal interface IWebBrowserService
{
    bool ProfileExist(string name);

    void OpenUrl(string url);

    void OpenUrl(string url, string profile);
}