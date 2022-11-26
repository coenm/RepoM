namespace RepoM.Core.Plugin.RepositoryActions.Actions;

using System;
using RepoM.Core.Plugin.RepositoryActions;

public class DelegateAction : IAction
{
    public DelegateAction(Action<object?, object>? action)
    {
        Action = action;
    }

    public Action<object?, object> Action { get; }
}