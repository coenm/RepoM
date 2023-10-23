namespace RepoM.ActionMenu.Core.Model.Functions;

using System;
using System.Collections;
using RepoM.ActionMenu.Interface.Attributes;
using RepoM.Core.Plugin.Repository;

[ActionMenuModule("Repository")]
internal partial class RepositoryFunctions : ScribanModuleWithFunctions
{
    private readonly IRepository _repository;

    public RepositoryFunctions(IRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        RegisterFunctions();
    }

    /// <summary>
    /// Gets the name of the repository.
    /// </summary>
    /// <returns>The name of the repository.</returns>
    /// <example>
    /// <code>
    /// repository.name
    /// </code>
    /// <code>
    /// "RepoM"
    /// </code>
    /// </example>
    [ActionMenuMember("name")]
    public string Name => _repository.Name;

    /// <summary>
    /// Gets the path of the repository.
    /// </summary>
    /// <returns>The path of the repository.</returns>
    [ActionMenuMember("path")]
    public string Path => _repository.Path;

    /// <summary>
    /// Gets the safe path of the repository.
    /// </summary>
    /// <returns>The path of the repository.</returns>
    [ActionMenuMember("safe_path")]
    public string SafePath => _repository.SafePath;

    /// <summary>
    /// Gets the Location of the repository.
    /// </summary>
    /// <returns>The path of the repository.</returns>
    [ActionMenuMember("location")]
    public string Location => _repository.Location;

    /// <summary>
    /// Gets the current branch of the repository
    /// </summary>
    /// <returns>The name of the current branch.</returns>
    [ActionMenuMember("branch")]
    public string CurrentBranch => _repository.CurrentBranch;

    /// <summary>
    /// Gets the current branch of the repository
    /// </summary>
    /// <returns>The name of the current branch.</returns>
    [ActionMenuMember("branches")]
    public IEnumerable Branches => _repository.Branches;

    /// <summary>
    /// Gets the local branches
    /// </summary>
    /// <returns>All local branches.</returns>
    [ActionMenuMember("local_branches")]
    public IEnumerable LocalBranches => _repository.LocalBranches;
    
    /// <summary>
    /// Gets the remotes.
    /// </summary>
    /// <returns>Remotes.</returns>
    [ActionMenuMember("remotes")]
    public IEnumerable Remotes => _repository.Remotes;
}