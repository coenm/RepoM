namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Action to pin (or unpin) the current repository. Pinning is not persistant and all pinned repositories will be cleared when RepoM exits.
/// Pinning a repository allowed custom filtering, ordering and searching.
/// </summary>
[RepositoryAction(TYPE)]
public sealed class RepositoryActionPinRepositoryV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "pin-repository@1";

    // hide base property only for the documentation.
    /// <summary>
    /// Name of the action. This is shown in the UI of RepoM. When no value is provided, the name will be a default value based on the mode.
    /// </summary>
    [EvaluatedProperty]
    [PropertyType(typeof(string))]
    public new string? Name { get; set; }
    
    /// <summary>
    /// The pin mode `[Toggle, Pin, UnPin]`.
    /// </summary>
    [Required]
    [PropertyType(typeof(PinMode))]
    public PinMode Mode { get; set; }

    /// <summary>
    /// The PinModes
    /// </summary>
    public enum PinMode
    {
        // when deserialization fails, this is the value.
        Unknown,

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