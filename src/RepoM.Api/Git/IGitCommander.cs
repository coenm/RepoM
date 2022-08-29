namespace RepoM.Api.Git;

using System;

public interface IGitCommander
{
    string Command(Repository repository, params string[] command);

    string CommandOneline(Repository repository, params string[] command);

    void CommandNoisy(Repository repository, params string[] command);

    void CommandOutputPipe(Repository repository, Action<string> handleOutput, params string[] command);
}