namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// TODO
/// </summary>
[RepositoryAction(TYPE)]
public sealed class RepositoryActionAssociateFileV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "associate-file@1";

    /// <summary>
    /// The extension of the file. Should be without the dot (`.`).
    /// </summary>
    // [EvaluatedProperty] //TODO
    [Required]
    [PropertyType(typeof(string))]
    public string? Extension { get; set; }
}