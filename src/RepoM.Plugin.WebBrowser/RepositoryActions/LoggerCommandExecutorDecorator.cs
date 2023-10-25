namespace RepoM.Plugin.WebBrowser.RepositoryActions;

using System;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Plugin.WebBrowser.RepositoryActions.Actions;

[UsedImplicitly]
internal class CoreBrowseRepositoryCommandExecutorDecorator : ICommandExecutor<Core.Plugin.RepositoryActions.Commands.BrowseRepositoryCommand>
{
    private readonly ICommandExecutor<BrowseRepositoryCommand> _pluginBrowseCommandExecutor;

    public CoreBrowseRepositoryCommandExecutorDecorator(
        ICommandExecutor<Core.Plugin.RepositoryActions.Commands.BrowseRepositoryCommand> decoratee,
        ICommandExecutor<BrowseRepositoryCommand> pluginBrowseCommandExecutor)

    {
        _pluginBrowseCommandExecutor = pluginBrowseCommandExecutor ?? throw new ArgumentNullException(nameof(pluginBrowseCommandExecutor));
        _ = decoratee;
    }

    public void Execute(IRepository repository, Core.Plugin.RepositoryActions.Commands.BrowseRepositoryCommand action)
    {
        _pluginBrowseCommandExecutor.Execute(repository, new BrowseRepositoryCommand(action.Url, null));
    }
}