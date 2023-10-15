namespace RepoM.Plugin.WebBrowser.RepositoryActions;

using System;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Plugin.WebBrowser.RepositoryActions.Actions;
using RepoM.Plugin.WebBrowser.Services;

[UsedImplicitly]
internal class BrowseActionExecutor : IActionExecutor<BrowseRepositoryCommand>
{
    private readonly IWebBrowserService _webBrowserService;

    public BrowseActionExecutor(IWebBrowserService webBrowserService)
    {
        _webBrowserService = webBrowserService ?? throw new ArgumentNullException(nameof(webBrowserService));
    }

    public void Execute(IRepository repository, BrowseRepositoryCommand repositoryCommand)
    {
        if (repositoryCommand.ProfileName == null)
        {
            _webBrowserService.OpenUrl(repositoryCommand.Url);
        }
        else
        {
            _webBrowserService.OpenUrl(repositoryCommand.Url, repositoryCommand.ProfileName);
        }
    }
}