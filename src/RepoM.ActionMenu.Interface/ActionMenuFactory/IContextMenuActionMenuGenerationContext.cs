namespace RepoM.ActionMenu.Interface.ActionMenuFactory;

public interface IContextMenuActionMenuGenerationContext : ITemplateEvaluator
{
    IScope CreateGlobalScope();
}