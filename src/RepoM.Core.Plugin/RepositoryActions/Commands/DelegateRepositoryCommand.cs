namespace RepoM.Core.Plugin.RepositoryActions.Commands;

using System;
using RepoM.Core.Plugin.RepositoryActions;

public sealed class DelegateRepositoryCommand : IRepositoryCommand
{
    public DelegateRepositoryCommand(Action<object?, object?> action)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
    }

    public Action<object?, object?> Action { get; }
}