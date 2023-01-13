namespace RepoM.Plugin.Clipboard.RepositoryAction;

using System;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Plugin.Clipboard.RepositoryAction.Actions;
using TextCopy;

[UsedImplicitly]
public class CopyToClipboardActionExecutor : IActionExecutor<CopyToClipboardAction>
{
    private readonly IClipboard _clipboard;

    public CopyToClipboardActionExecutor(IClipboard clipboard)
    {
        _clipboard = clipboard ?? throw new ArgumentNullException(nameof(clipboard));
    }

    public void Execute(IRepository repository, CopyToClipboardAction action)
    {
        var txt = !string.IsNullOrWhiteSpace(action.Text) ? action.Text : string.Empty;
        _clipboard.SetText(txt);
    }
}