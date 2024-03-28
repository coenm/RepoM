namespace RepoM.ActionMenu.Core.ActionMenu.Context;

using System;
using System.Collections;
using RepoM.ActionMenu.Core.Model;
using RepoM.ActionMenu.Interface.Attributes;
using RepoM.Core.Plugin.Repository;

/// <summary>
/// Provides action menu functions and variables for the current repository through `repository`.
/// </summary>
[ActionMenuContext("repository")]
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
    /// Gets the path of the repository. The path is windows or linux based (depending on the running OS) and does NOT end with a (back)slash.
    /// </summary>
    /// <returns>The repository path.</returns>
    [ActionMenuContextMember("path")]
    public string Path => Environment.OSVersion.Platform switch
        {
            PlatformID.Win32NT => WindowsPath,
            PlatformID.Unix => LinuxPath,
            _ => string.Empty,
        };

    /// <summary>
    /// Gets the path of the repository in windows style (i.e. use `/`). The path does NOT end with a slash.
    /// </summary>
    /// <returns>The path of the repository.</returns>
    [ActionMenuContextMember("windows_path")]
    public string WindowsPath => _repository.WindowsPath;

    /// <summary>
    /// Gets the path of the repository in linux style (i.e. use `\`). The path does NOT end with a backslash.
    /// </summary>
    /// <returns>The backslash based path of the repository without the last backslash.</returns>
    [ActionMenuContextMember("linux_path")]
    public string LinuxPath => _repository.LinuxPath;
    
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