namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.ForEach;

using System.Collections;
using System.Collections.Generic;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;

internal class RepositoryActionForEachV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionForEachV1>
{
    protected override async IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(RepositoryActionForEachV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        if (string.IsNullOrWhiteSpace(action.Variable))
        {
            yield break;
        }

        var result = await action.Enumerable.EvaluateAsync(context);

        if (result is not IEnumerable enumerable)
        {
            yield break;
        }

        foreach (var item in enumerable)
        {
            if (item is null)
            {
                continue;
            }

            using IScope scope = context.CreateGlobalScope();
            
            // todo evaluate to string or not?
            scope.SetValue(action.Variable, item, true);

            if (await action.Skip.EvaluateAsync(context).ConfigureAwait(false))
            {
                continue;
            }

            IEnumerable<UserInterfaceRepositoryActionBase> items = await context.AddActionMenusAsync(action.Actions).ConfigureAwait(false);

            if (items is null)
            {
                continue;
            }

            foreach (UserInterfaceRepositoryActionBase menuItem in items)
            {
                yield return menuItem;
            }
        }
    }
}