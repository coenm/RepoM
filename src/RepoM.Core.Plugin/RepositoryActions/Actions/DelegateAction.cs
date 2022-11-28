namespace RepoM.Core.Plugin.RepositoryActions.Actions;

using System;
using RepoM.Core.Plugin.RepositoryActions;

public sealed class DelegateAction : IAction
{
    public DelegateAction(Action<object?, object?> action)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
    }

    public Action<object?, object?> Action { get; }
}