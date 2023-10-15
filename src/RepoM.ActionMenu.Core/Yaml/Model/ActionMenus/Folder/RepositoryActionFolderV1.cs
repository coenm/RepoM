namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Folder;

using RepoM.ActionMenu.Core.Yaml.Model.ActionContext;
using RepoM.ActionMenu.Interface.YamlModel;

internal sealed class RepositoryActionFolderV1 : IMenuAction, IName, IMenuActions, IContext, IDeferred
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE_VALUE = "folder@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    public ActionMenu? Actions { get; init; }

    public string Name { get; init; } = string.Empty;
   
    public string? Active { get; init; }

    public Context? Context { get; init; }

    public string? IsDeferred { get; init; }

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name} : #actions: {Actions?.Count ?? 0}";
    }
}