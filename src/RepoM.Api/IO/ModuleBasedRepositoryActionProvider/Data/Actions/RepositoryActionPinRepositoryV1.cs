namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// TODO
/// </summary>
[RepositoryAction(TYPE)]
public sealed class RepositoryActionPinRepositoryV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "pin-repository@1";

    /// <summary>
    /// The pin mode [Toggle, Pin, UnPin].
    /// </summary>
    //[EvaluatedProperty] //todo
    [Required]
    [PropertyType(typeof(string))]
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