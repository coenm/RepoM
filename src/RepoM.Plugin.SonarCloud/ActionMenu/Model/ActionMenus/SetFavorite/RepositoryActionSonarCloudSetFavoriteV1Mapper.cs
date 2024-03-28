namespace RepoM.Plugin.SonarCloud.ActionMenu.Model.ActionMenus.SetFavorite;

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.SonarCloud.RepositoryCommands;

[UsedImplicitly]
internal class RepositoryActionSonarCloudSetFavoriteV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionSonarCloudSetFavoriteV1>
{
    private readonly ISonarCloudFavoriteService _service;
    private readonly ILogger _logger;

    public RepositoryActionSonarCloudSetFavoriteV1Mapper(ISonarCloudFavoriteService service, ILogger logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(RepositoryActionSonarCloudSetFavoriteV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        if (_service.IsInitialized)
        {
            var name = await action.Name.RenderAsync(context).ConfigureAwait(false);
            var key = await action.Project.RenderAsync(context).ConfigureAwait(false);

            yield return new UserInterfaceRepositoryAction(name, repository)
                {
                    RepositoryCommand = new SonarCloudSetFavoriteRepositoryCommand(key),
                    ExecutionCausesSynchronizing = false,
                };
        }
        else
        {
            _logger.LogDebug("SonarCloud service is not initialized. Therefore, no menu item");
        }
    }
}