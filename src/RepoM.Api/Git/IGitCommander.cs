namespace RepoM.Api.Git;

using System;
using RepoM.Core.Plugin.Repository;

public interface IGitCommander
{
    string Command(IRepository repository, params string[] command);

    void CommandNoisy(IRepository repository, params string[] command);

    void CommandOutputPipe(IRepository repository, Action<string> handleOutput, params string[] command);
}