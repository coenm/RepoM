namespace RepoM.ActionMenu.CodeGen;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RepoM.ActionMenu.CodeGen.Models;
using Scriban;
using Scriban.Runtime;

internal static class DocumentationGenerator
{
    public static async Task<string> GetPluginDocsContentAsync(ProjectDescriptor plugin, Template template)
    {
        var context = new TemplateContext
            {
                LoopLimit = 0,
                MemberRenamer = x => x.Name,
            };

        var scriptObject = new ScriptObject()
            {
                { "plugin", plugin },
            };

        context.PushGlobal(scriptObject);

        return await template.RenderAsync(context);
    }

    public static async Task<string> GetDocsContentAsync(ActionMenuContextClassDescriptor module, Template template)
    {
        module.Members.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.Ordinal));

        //module.Title = $"{module.Name} {(module.IsBuiltin ? "Functions" : "Module")}";

        var context = new TemplateContext
            {
                LoopLimit = 0,
                MemberRenamer = x => x.Name,
            };

        var scriptObject = new ScriptObject()
            {
                { "module", module },
            };

        context.PushGlobal(scriptObject);

        return await template.RenderAsync(context);
    }

    public static async Task<string> GetScribanInitializersCSharpCodeAsync(IEnumerable<ActionMenuContextClassDescriptor> actionContextMenus, Template templateModule)
    {
        var modules = actionContextMenus.OrderBy(x => x.ClassName).ToList();

        var context = new TemplateContext
            {
                LoopLimit = 0,
                MemberRenamer = x => x.Name,
                EnableRelaxedMemberAccess = false,
            };

        var scriptObject = new ScriptObject()
            {
                { "modules", modules },
            };

        context.PushGlobal(scriptObject);

        return await templateModule.RenderAsync(context).ConfigureAwait(false);
    }
}