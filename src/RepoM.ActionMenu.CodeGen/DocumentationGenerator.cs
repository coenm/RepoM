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
    public static async Task<string> GetPluginDocsContentAsync(ProjectDescriptor plugin, Template template, string name)
    {
        plugin.ActionMenus.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.Ordinal));

        var context = new TemplateContext
        {
            LoopLimit = 0,
            MemberRenamer = x => x.Name,
            TemplateLoader = new FileSystemTemplateLoader(name, "Templates/Parts", RepoMFolders.DocumentationMarkDownSource),
        };

        var scriptObject = new ScriptObject
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
            TemplateLoader = new FileSystemTemplateLoader(string.Empty, "Templates/Parts", RepoMFolders.DocumentationMarkDownSource),
        };

        var scriptObject = new ScriptObject
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
            TemplateLoader = new FileSystemTemplateLoader(string.Empty, "Templates/Parts", RepoMFolders.DocumentationMarkDownSource),
        };

        var scriptObject = new ScriptObject
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
    private readonly string _prefix;
    private readonly string _scribanTemplatesPathBase;
    private readonly string _docsIncludeSourceFullPath;

    public FileSystemTemplateLoader(string prefix, string scribanTemplatesPath, string docsIncludeSourceFullPath)
    {
        _prefix = prefix;
        _scribanTemplatesPathBase = scribanTemplatesPath ?? throw new ArgumentNullException(nameof(scribanTemplatesPath));
        _docsIncludeSourceFullPath = docsIncludeSourceFullPath ?? throw new ArgumentNullException(nameof(docsIncludeSourceFullPath));
    }

    public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
    {
        if (!string.IsNullOrWhiteSpace(_prefix) && templateName.Length == 2)
        {
            if (int.TryParse(templateName, out _))
            {
                var prefix = Path.Combine(_docsIncludeSourceFullPath, $"{_prefix}." + templateName);
                var path = $"{prefix}.md";
                if (File.Exists(path))
                {
                    return path;
                }
                
                return string.Empty;
            }
        }

        var result = Path.Combine(_scribanTemplatesPathBase, templateName);
        return result;
    }

    public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        if (string.IsNullOrEmpty(templatePath))
        {
            return string.Empty;
        }

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
        if (string.IsNullOrEmpty(templatePath))
        {
            return string.Empty;
        }

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