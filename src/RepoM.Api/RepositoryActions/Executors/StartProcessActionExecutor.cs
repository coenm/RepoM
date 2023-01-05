namespace RepoM.Api.RepositoryActions.Executors;

using JetBrains.Annotations;
using RepoM.Api.IO;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Actions;

[UsedImplicitly]
public class StartProcessActionExecutor : IActionExecutor<StartProcessAction>
{
    public void Execute(IRepository repository, StartProcessAction action)
    {
        var args = string.Empty;

        if (action.Arguments.Length == 1)
        {
            args = action.Arguments[0];
        }
        else if (action.Arguments.Length > 1)
        {
            args = string.Join(' ', action.Arguments);
        }

        ProcessHelper.StartProcess(action.Executable, args);
    }
}