namespace RepoM.Plugin.Clipboard.RepositoryAction.Actions;

using RepoM.Core.Plugin.RepositoryActions;

public sealed class CopyToClipboardRepositoryCommand : IRepositoryCommand
{
    internal CopyToClipboardRepositoryCommand(string? text)
    {
        Text = text;
    }

    public string? Text { get; }
}