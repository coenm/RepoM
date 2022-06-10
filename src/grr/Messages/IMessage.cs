namespace Grr.Messages;

using RepoM.Ipc;

public interface IMessage
{
    string? GetRemoteCommand();

    void Execute(Repository[] repositories);

    bool HasRemoteCommand { get; }

    bool ShouldWriteRepositories(Repository[] repositories);
}