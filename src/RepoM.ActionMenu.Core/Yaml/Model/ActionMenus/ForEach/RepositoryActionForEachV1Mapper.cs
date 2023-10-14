namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.ForEach;

using System.Collections;
using System.Collections.Generic;
using RepoM.ActionMenu.Core.Model;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;

internal class RepositoryActionForEachV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionForEachV1>
{
    protected override async IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(RepositoryActionForEachV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        if (string.IsNullOrWhiteSpace(action.Enumerable))
        {
            yield break;
        }

        if (string.IsNullOrWhiteSpace(action.Variable))
        {
            yield break;
        }

        var enumerable = await context.EvaluateAsync(action.Enumerable).ConfigureAwait(false);
        if (enumerable is null)
        {
            yield break;
        }

        if (enumerable is not IList list)
        {
            yield break;
        }

        if (list.Count == 0)
        {
            yield break;
        }
        
        foreach (var item in list)
        {
            if (item is null)
            {
                continue;
            }

            using var scope = context.CreateGlobalScope();
            
            // todo evaluate to string or not?
            scope.SetValue(action.Variable, item, true);

            if (await context.EvaluateToBooleanAsync(action.Skip, false).ConfigureAwait(false))
            {
                continue;
            }

            var items = await context.AddActionMenusAsync(action.Actions).ConfigureAwait(false);

            if (items is null)
            {
                continue;
            }

            foreach (var menuItem in items)
            {
                yield return menuItem;
            }
        }
    }
}