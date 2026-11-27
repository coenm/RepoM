namespace RepoM.Api.Git;

using System;
using System.Threading;
using System.Threading.Tasks;
using RepoM.Core.Plugin.Repository;

public interface IGitCommander
{
    Task<string> CommandAsync(string workingDirectory, CancellationToken cancellationToken, params string[] command);

    string Command(IRepository repository, params string[] command);

    void CommandNoisy(IRepository repository, params string[] command);

    void CommandOutputPipe(IRepository repository, Action<string> handleOutput, params string[] command);
}