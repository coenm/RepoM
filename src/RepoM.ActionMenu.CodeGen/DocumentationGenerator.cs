namespace RepoM.ActionMenu.CodeGen;

using System;
using System.Threading.Tasks;
using RepoM.ActionMenu.CodeGen.Models;
using Scriban;
using Scriban.Runtime;

internal static class DocumentationGenerator
{
    public static async Task<string> GetDocsContentAsync(KalkModuleToGenerate module, Template template)
    {
        if (module.Name == "KalkEngine")
        {
            module.Name = "General";
        }

        module.Members.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.Ordinal));

        module.Title = $"{module.Name} {(module.IsBuiltin ? "Functions" : "Module")}";

        var name = module.Name.ToLowerInvariant();
        module.Url = $"/doc/api/{name}/";

        var context = new TemplateContext
            {
                LoopLimit = 0,
            };
        var scriptObject = new ScriptObject()
            {
                { "module", module },
            };
        context.PushGlobal(scriptObject);
        context.MemberRenamer = x => x.Name;
        return await template.RenderAsync(context);
    }
}