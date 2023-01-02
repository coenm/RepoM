namespace RepoM.Plugin.Heidi.RepositoryActions;

using JetBrains.Annotations;
using RepoM.Api.IO;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;

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