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
    Task<IEnumerable<UserInterfaceRepositoryActionBase>> AddActionMenusAsync(List<IMenuAction>? actionActions);

    IScope CreateGlobalScope();

    IActionMenuGenerationContext Clone();
}