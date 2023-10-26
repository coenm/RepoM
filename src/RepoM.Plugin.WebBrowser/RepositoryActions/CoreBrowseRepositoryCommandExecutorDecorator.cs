namespace RepoM.Plugin.WebBrowser.RepositoryActions;

using System;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;

using CoreCommand = Core.Plugin.RepositoryActions.Commands;
using PluginCommand = Actions;

/// <remarks>Must be public because decorator.</remarks>
[UsedImplicitly]
public class CoreBrowseRepositoryCommandExecutorDecorator : ICommandExecutor<CoreCommand.BrowseRepositoryCommand>
{
    private readonly ICommandExecutor<PluginCommand.BrowseRepositoryCommand> _pluginBrowseCommandExecutor;

    public CoreBrowseRepositoryCommandExecutorDecorator(
        ICommandExecutor<CoreCommand.BrowseRepositoryCommand> decoratee,
        ICommandExecutor<PluginCommand.BrowseRepositoryCommand> pluginBrowseCommandExecutor)

    {
        _pluginBrowseCommandExecutor = pluginBrowseCommandExecutor ?? throw new ArgumentNullException(nameof(pluginBrowseCommandExecutor));
        _ = decoratee; // ignore the decoratee because we are to remap command.
    }

    public void Execute(IRepository repository, CoreCommand.BrowseRepositoryCommand action)
    {
        _pluginBrowseCommandExecutor.Execute(repository, new PluginCommand.BrowseRepositoryCommand(action.Url, null));
    }
}