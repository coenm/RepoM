namespace RepoM.Plugin.Clipboard.RepositoryAction.Actions;

using RepoM.Core.Plugin.RepositoryActions;

public sealed class CopyToClipboardAction : IAction
{
    internal CopyToClipboardAction(string? text)
    {
        Text = text;
    }

    public string? Text { get; }
}