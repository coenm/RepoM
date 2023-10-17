namespace RepoM.Plugin.Clipboard.RepositoryAction;

using System;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Plugin.Clipboard.RepositoryAction.Actions;
using TextCopy;

[UsedImplicitly]
internal class CopyToClipboardCommandExecutor : ICommandExecutor<CopyToClipboardRepositoryCommand>
{
    private readonly IClipboard _clipboard;

    public CopyToClipboardCommandExecutor(IClipboard clipboard)
    {
        _clipboard = clipboard ?? throw new ArgumentNullException(nameof(clipboard));
    }

    public void Execute(IRepository repository, CopyToClipboardRepositoryCommand repositoryCommand)
    {
        var txt = !string.IsNullOrWhiteSpace(repositoryCommand.Text) ? repositoryCommand.Text : string.Empty;
        _clipboard.SetText(txt);
    }
}