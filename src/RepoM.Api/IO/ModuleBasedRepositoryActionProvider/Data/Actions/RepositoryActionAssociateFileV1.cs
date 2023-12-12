namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

using System;
using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Interface.YamlModel;

/// <summary>
/// Action menu for opening files with a given extension. If files within the repository are found matching the extension, a submenu will be created with all matched files.
/// </summary>
[RepositoryAction(TYPE)]
[Obsolete("Old action menu")]
public sealed class RepositoryActionAssociateFileV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "associate-file@1";

    /// <summary>
    /// The file extension to look for. This parameter can contain a combination of valid literal path and wildcard (`*` and `?`) characters, but it doesnt support regular expressions.
    /// For example `*.sln`.
    /// </summary>
    [Required]
    [PropertyType(typeof(string))]
    public string? Extension { get; set; }
}