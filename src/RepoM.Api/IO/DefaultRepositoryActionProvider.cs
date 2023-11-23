namespace RepoM.Api.IO;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.RepositoryActions;

public class DefaultRepositoryActionProvider : IRepositoryActionProvider
{
    private readonly RepositorySpecificConfiguration _repoSpecificConfig;
    private readonly ILogger _logger;

    public DefaultRepositoryActionProvider(RepositorySpecificConfiguration repoSpecificConfig, ILogger logger)
    {
        _repoSpecificConfig = repoSpecificConfig ?? throw new ArgumentNullException(nameof(repoSpecificConfig));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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