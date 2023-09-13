namespace RepoM.Plugin.WebBrowser.RepositoryActions;

using System;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Plugin.WebBrowser.RepositoryActions.Actions;
using RepoM.Plugin.WebBrowser.Services;

[UsedImplicitly]
internal class BrowseActionExecutor : IActionExecutor<BrowseAction>
{
    private readonly IWebBrowserService _webBrowserService;

    public BrowseActionExecutor(IWebBrowserService webBrowserService)
    {
        _webBrowserService = webBrowserService ?? throw new ArgumentNullException(nameof(webBrowserService));
    }

    public void Execute(IRepository repository, BrowseAction action)
    {
        if (action.ProfileName == null)
        {
            _webBrowserService.OpenUrl(action.Url);
        }
        else
        {
            _webBrowserService.OpenUrl(action.Url, action.ProfileName);
        }
    }
}