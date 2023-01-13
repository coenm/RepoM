namespace RepoM.Plugin.Clipboard.ActionProvider;

using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

public class RepositoryActionClipboardCopyV1 : RepositoryAction
{
    public string? Text { get; set; }
}