namespace RepoM.ActionMenu.CodeGen;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RepoM.ActionMenu.CodeGen.Models;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

internal static class DocumentationGenerator
{
    private static readonly FileSystemTemplateLoader _templateLoader = new("Templates/Parts");

    public static async Task<string> GetPluginDocsContentAsync(ProjectDescriptor plugin, Template template)
    {
        plugin.ActionMenus.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.Ordinal));

        var context = new TemplateContext
            {
                LoopLimit = 0,
                MemberRenamer = x => x.Name,
                TemplateLoader = _templateLoader,
        };

        var scriptObject = new ScriptObject()
            {
                { "plugin", plugin },
            };
        scriptObject.Import(typeof(MyStringFunctions));

        context.PushGlobal(scriptObject);

        return await template.RenderAsync(context);
    }

    public static async Task<string> GetDocsContentAsync(ActionMenuContextClassDescriptor module, Template template)
    {
        module.Members.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.Ordinal));

        var context = new TemplateContext
            {
                LoopLimit = 0,
                MemberRenamer = x => x.Name,
                TemplateLoader = _templateLoader,
        };

        var scriptObject = new ScriptObject()
            {
                { "module", module },
            };
        scriptObject.Import(typeof(MyStringFunctions));

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
                TemplateLoader = _templateLoader,
        };

        var scriptObject = new ScriptObject()
            {
                { "modules", modules },
            };
        scriptObject.Import(typeof(MyStringFunctions));

        context.PushGlobal(scriptObject);

        return await templateModule.RenderAsync(context).ConfigureAwait(false);
    }
}

internal class FileSystemTemplateLoader : ITemplateLoader
{
    private readonly string _basePath;

    public FileSystemTemplateLoader(string basePath)
    {
        _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
    }

    public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
    {
        var result = Path.Combine(_basePath, templateName);
        return result;
    }

    public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        try
        {
            var result = File.ReadAllText(templatePath);
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        try
        {
            var result = await File.ReadAllTextAsync(templatePath);
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}

public static class MyStringFunctions
{
    public static string Hyphenated(string input)
    {
        return string.Concat(input.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x : x.ToString())).ToLowerInvariant();
    }
}