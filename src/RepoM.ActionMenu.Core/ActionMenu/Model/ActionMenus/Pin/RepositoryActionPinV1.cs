namespace RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.Pin;

using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

/// <summary>
/// Action to pin (or unpin) the current repository. Pinning is not persistent and all pinned repositories will be cleared when RepoM exits.
/// Pinning a repository allowed custom filtering, ordering and searching.
/// </summary>
/// <example>
/// <snippet name='pin-repository@1-scenario01' mode='snippet' />
/// </example>
[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionPinV1 : IMenuAction, IContext, IName
{
    public const string TYPE_VALUE = "pin-repository@1";
    internal const string EXAMPLE_1 = TYPE_VALUE + "-scenario01";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <inheritdoc cref="IName.Name"/>
    [Text("Pin / Unpin Repo")]
    public Text Name { get; set; } = null!;

    /// <inheritdoc cref="IMenuAction.Active"/>
    [Predicate(true)]
    public Predicate Active { get; set; } = new ScribanPredicate();

    /// <inheritdoc cref="IContext.Context"/>
    public Context? Context { get; set; }

    /// <summary>
    /// The pin mode `[Toggle, Pin, UnPin]`.
    /// </summary>
    public PinMode? Mode { get; set; } = PinMode.Toggle; // GitHub issue: https://github.com/coenm/RepoM/issues/87

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