namespace RepoM.Api.IO;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.Logging;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;

public class DefaultRepositoryActionProvider : IRepositoryActionProvider
{
    private readonly IFileSystem _fileSystem;
    private readonly RepositorySpecificConfiguration _repoSpecificConfig;
    private readonly ILogger _logger;

    public DefaultRepositoryActionProvider(
        IFileSystem fileSystem,
        RepositorySpecificConfiguration repoSpecificConfig,
        ILogger logger)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _repoSpecificConfig = repoSpecificConfig ?? throw new ArgumentNullException(nameof(repoSpecificConfig));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public RepositoryActionBase? GetPrimaryAction(Repository repository)
    {
        return GetContextMenuActions(new[] { repository, }).FirstOrDefault();
    }

    public RepositoryActionBase? GetSecondaryAction(Repository repository)
    {
        RepositoryActionBase[] actions = GetContextMenuActions(new[] { repository, }).Take(2).ToArray();
        return actions.Length > 1 ? actions[1] : null;
    }

    public IEnumerable<RepositoryActionBase> GetContextMenuActions(IEnumerable<Repository> repositories)
    {
        return GetContextMenuActionsInternal(repositories.Where(r => _fileSystem.Directory.Exists(r.SafePath))).Where(a => a != null);
    }

    private IEnumerable<RepositoryActionBase> GetContextMenuActionsInternal(IEnumerable<Repository> repos)
    {
        Repository[] repositories = repos.ToArray();

        try
        {
            return _repoSpecificConfig.CreateActions(repositories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not create action menu.");
            throw;
        }
    }
}