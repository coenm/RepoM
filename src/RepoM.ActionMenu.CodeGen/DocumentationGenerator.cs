namespace RepoM.ActionMenu.CodeGen;

using System;
using System.Threading.Tasks;
using RepoM.ActionMenu.CodeGen.Models;
using RepoM.ActionMenu.CodeGen.Models.New;
using Scriban;
using Scriban.Runtime;

internal static class DocumentationGenerator
{
    public static async Task<string> GetDocsContentAsyncNew(ActionMenuContextClassDescriptor module, Template template)
    {
        module.Members.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.Ordinal));

        //module.Title = $"{module.Name} {(module.IsBuiltin ? "Functions" : "Module")}";

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