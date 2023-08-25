namespace RepoM.Api.IO;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;

public class DefaultRepositoryActionProvider : IRepositoryActionProvider
{
    private readonly RepositorySpecificConfiguration _repoSpecificConfig;
    private readonly ILogger _logger;

    public DefaultRepositoryActionProvider(RepositorySpecificConfiguration repoSpecificConfig, ILogger logger)
    {
        _repoSpecificConfig = repoSpecificConfig ?? throw new ArgumentNullException(nameof(repoSpecificConfig));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public RepositoryActionBase? GetPrimaryAction(Repository repository)
    {
        return GetContextMenuActions(repository).FirstOrDefault();
    }

    public RepositoryActionBase? GetSecondaryAction(Repository repository)
    {
        RepositoryActionBase[] actions = GetContextMenuActions(repository).Take(2).ToArray();
        return actions.Length > 1 ? actions[1] : null;
    }

    public IEnumerable<RepositoryActionBase> GetContextMenuActions(Repository repository)
    {
        return GetContextMenuActionsInternal(repository);
    }

    private IEnumerable<RepositoryActionBase> GetContextMenuActionsInternal(Repository repository)
    {
        try
        {
            return _repoSpecificConfig.CreateActions(repository);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not create action menu.");
            throw;
        }
    }
}