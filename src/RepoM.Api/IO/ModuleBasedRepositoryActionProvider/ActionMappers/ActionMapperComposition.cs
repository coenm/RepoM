namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Git;
using RepoM.Api.RepositoryActions;
using RepoM.Core.Plugin.Expressions;
using RepositoryAction = RepositoryActions.RepositoryAction;

public class ActionMapperComposition
{
    private readonly IActionToRepositoryActionMapper[] _deserializers;

    public ActionMapperComposition(IEnumerable<IActionToRepositoryActionMapper> deserializers, IRepositoryExpressionEvaluator repoExpressionEvaluator)
    {
        _deserializers = deserializers.ToArray();
    }

    public RepositoryActionBase[] Map(Data.RepositoryAction action, Repository repository)
    {
        if (repository == null)
        {
            throw new ArgumentNullException(nameof(repository));
        }

        IActionToRepositoryActionMapper? deserializer = Array.Find(_deserializers, item => item.CanMap(action));

        IEnumerable<RepositoryActionBase> result = deserializer?.Map(action, repository, this) ?? Enumerable.Empty<RepositoryAction>();

        return result.ToArray();
    }
}