namespace RepoM.ActionMenu.Interface.ActionMenuFactory;

using System.Threading.Tasks;

public interface ITemplateEvaluator
{
    Task<string> RenderStringAsync(string text);

    Task<object> EvaluateAsync(string text);
}