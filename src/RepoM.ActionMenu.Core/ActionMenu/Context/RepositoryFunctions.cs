namespace RepoM.ActionMenu.Core.ActionMenu.Context;

using System;
using System.Collections;
using RepoM.ActionMenu.Core.Model;
using RepoM.ActionMenu.Interface.Attributes;
using RepoM.Core.Plugin.Repository;

/// <summary>
/// Provides action menu functions and variables for the current repository through `repository`.
/// </summary>
[ActionMenuContext("Repository")]
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
    /// <usage/>
    /// <code>
    /// repository.name
    /// </code>
    /// </example>
    [ActionMenuContextMember("name")]
    public string Name => _repository.Name;

    /// <summary>
    /// Gets the path of the repository.
    /// </summary>
    /// <returns>The path of the repository.</returns>
    [ActionMenuContextMember("path")]
    public string Path => _repository.Path;

    /// <summary>
    /// Gets the safe path of the repository.
    /// </summary>
    /// <returns>The path of the repository.</returns>
    [ActionMenuContextMember("safe_path")]
    public string SafePath => _repository.SafePath;

    /// <summary>   
    /// Gets the Location of the repository.
    /// </summary>
    /// <returns>The path of the repository.</returns>
    [ActionMenuContextMember("location")]
    public string Location => _repository.Location;

    /// <summary>
    /// Gets the current branch of the repository
    /// </summary>
    /// <returns>The name of the current branch.</returns>
    [ActionMenuContextMember("branch")]
    public string CurrentBranch => _repository.CurrentBranch;

    /// <summary>
    /// Gets the current branch of the repository
    /// </summary>
    /// <returns>The name of the current branch.</returns>
    [ActionMenuContextMember("branches")]
    public IEnumerable Branches => _repository.Branches;

    /// <summary>
    /// Gets the local branches
    /// </summary>
    /// <returns>All local branches.</returns>
    [ActionMenuContextMember("local_branches")]
    public IEnumerable LocalBranches => _repository.LocalBranches;
    
    /// <summary>
    /// Gets the remotes.
    /// </summary>
    /// <returns>Remotes.</returns>
    [ActionMenuContextMember("remotes")]
    public IEnumerable Remotes => _repository.Remotes;
}