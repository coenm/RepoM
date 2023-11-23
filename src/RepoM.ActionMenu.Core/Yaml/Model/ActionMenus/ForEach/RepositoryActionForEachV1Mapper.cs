namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.ForEach;

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;

[UsedImplicitly]
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

            if (action.IterationContext != null)
            {
                foreach (IContextAction ctx in action.IterationContext)
                {
                    await scope.AddContextActionAsync(ctx).ConfigureAwait(false);
                }
            }

            if (await action.Skip.EvaluateAsync(context).ConfigureAwait(false))
            {
                continue;
            }

            await foreach (UserInterfaceRepositoryActionBase menuItem in context.AddActionMenusAsync(action.Actions).ConfigureAwait(false))
            {
                yield return menuItem;
            }
        }
    }
}