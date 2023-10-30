namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Pin;

using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

/// <summary>
/// Action to pin (or unpin) the current repository. Pinning is not persistant and all pinned repositories will be cleared when RepoM exits.
/// Pinning a repository allowed custom filtering, ordering and searching.
/// </summary>
[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionPinV1 : IMenuAction, IContext, IName
{
    public const string TYPE_VALUE = "pin-repository@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <inheritdoc cref="IName.Name"/>
    [Text("(Un)Pin repository")]
    public Text Name { get; set; } = null!;

    /// <inheritdoc cref="IMenuAction.Active"/>
    [Predicate(true)]
    public Predicate Active { get; set; } = new ScribanPredicate();

    public Context? Context { get; set; }

    /// <summary>
    /// The pin mode `[Toggle, Pin, UnPin]`.
    /// </summary>
    public PinMode? Mode { get; set; } // todo scriban?!
    
    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Mode}";
    }

    /// <summary>
    /// The PinModes
    /// </summary>
    public enum PinMode
    {
        /// <summary>
        /// Toggle
        /// </summary>
        Toggle,

        /// <summary>
        /// Pin
        /// </summary>
        Pin,

        /// <summary>
        /// UnPin
        /// </summary>
        UnPin,
    }
}