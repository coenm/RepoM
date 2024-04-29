namespace RepoM.ActionMenu.Interface.ActionMenuFactory;

using System.Collections.Generic;
using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;

/// <summary>
/// Context for the generation of the action menus (to be used by mappers)
/// </summary>
public interface IActionMenuGenerationContext : ITemplateEvaluator, IMenuContext
{
    IAsyncEnumerable<UserInterfaceRepositoryActionBase> AddActionMenusAsync(List<IMenuAction>? menus);

    IScope CreateGlobalScope();

    IActionMenuGenerationContext Clone();
}

public static class ActionMenuGenerationContextExtensions
{
    public static async Task<UserInterfaceRepositoryActionBase[]> AddActionMenusAsyncArray(this IActionMenuGenerationContext instance, List<IMenuAction>? menus)
    {
        var list = new List<UserInterfaceRepositoryActionBase>();

        await foreach (UserInterfaceRepositoryActionBase item in instance.AddActionMenusAsync(menus).ConfigureAwait(false))
        {
            list.Add(item);
        }

        return [.. list, ];
    }
}