namespace RepoM.ActionMenu.CodeGen;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Broslyn;
using Kalk.CodeGen;
using Kalk.Core;
using Microsoft.CodeAnalysis;
using RepoM.ActionMenu.Interface.Attributes;
using Scriban;
using Scriban.Runtime;

public partial class Program
{
    private static readonly Regex _removeCode = RemoveCodeRegex();
    private static readonly Regex _promptRegex = PromptRegex();

    static async Task Main(string[] args)
    {
        // not sure why Kalk has this.
        _ = typeof(System.Composition.CompositionContext).Name;

        var rootFolder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../../../../.."));
        var srcFolder = Path.Combine(rootFolder, "src");
        var docsFolder = Path.Combine(rootFolder, "docs_new");
        var projectName = "RepoM.ActionMenu.Core";
        var pathToSolution = Path.Combine(srcFolder, projectName, $"{projectName}.csproj");
        var pathToGeneratedCode = Path.Combine(srcFolder, projectName, "RepoMCodeGen.generated.cs");

        if (!Directory.Exists(Path.Combine(rootFolder, ".git")))
        {
            throw new Exception("Wrong root folder");
        }
        
        if (!Directory.Exists(srcFolder))
        {
            throw new Exception($"src folder `{srcFolder}` doesn't exist");
        }
        
        if (!Directory.Exists(docsFolder))
        {
            throw new Exception($"docsFolder folder `{docsFolder}` doesn't exist");
        }
        
        if (!File.Exists(pathToSolution))
        {
            throw new Exception($"File `{pathToSolution}` does not exist");
        }

        CSharpCompilationCaptureResult compilationCaptureResult = CSharpCompilationCapture.Build(pathToSolution);
        Solution solution = compilationCaptureResult.Workspace.CurrentSolution;
        Project[] solutionProjects = solution.Projects.ToArray();
        Project project = Array.Find(solutionProjects, x => x.Name == projectName) ?? throw new Exception($"Project `{projectName}` not found in solution");

        // Make sure that doc will be parsed
        project = project.WithParseOptions(project.ParseOptions!.WithDocumentationMode(DocumentationMode.Parse));

        // Compile the project
        Compilation compilation = await project.GetCompilationAsync() ?? throw new Exception("Compilation failed");

        ImmutableArray<Diagnostic> diagnostics = compilation.GetDiagnostics();
        Diagnostic[] errors = diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length > 0)
        {
            Console.WriteLine("Compilation errors:");
            foreach (var error in errors)
            {
                Console.WriteLine(error);
            }

            Console.WriteLine("Error, Exiting.");
            Environment.Exit(1);
            return;
        }

        //var kalkEngine = compilation.GetTypeByMetadataName("Kalk.Core.KalkEngine");
        var mapNameToModule = new Dictionary<string, KalkModuleToGenerate>();

        void GetOrCreateModule(ITypeSymbol typeSymbol, string className, AttributeData moduleAttribute, out KalkModuleToGenerate moduleToGenerate)
        {
            var ns = typeSymbol.ContainingNamespace.ToDisplayString();

            var fullClassName = $"{ns}.{className}";
            if (!mapNameToModule.TryGetValue(fullClassName, out moduleToGenerate))
            {
                moduleToGenerate = new KalkModuleToGenerate()
                    {
                        Namespace = typeSymbol.ContainingNamespace.ToDisplayString(),
                        ClassName = className,
                    };
                mapNameToModule.Add(fullClassName, moduleToGenerate);

                if (moduleAttribute != null)
                {
                    moduleToGenerate.Name = moduleAttribute.ConstructorArguments[0].Value.ToString();
                    moduleToGenerate.Names.Add(moduleToGenerate.Name!);
                    moduleToGenerate.Category = "Modules (e.g `import Files`)";
                }
                else
                {
                    moduleToGenerate.Name = className.Replace("Module", "");
                    moduleToGenerate.IsBuiltin = true;
                }

                ExtractDocumentation(typeSymbol, moduleToGenerate);
            }
        }

        foreach (ISymbol type in compilation.GetSymbolsWithName(x => true, SymbolFilter.Type))
        {
            if (type is not ITypeSymbol typeSymbol)
            {
                continue;
            }
                
            var moduleAttribute = typeSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass.Name == nameof(ActionMenuModuleAttribute));
            KalkModuleToGenerate? moduleToGenerate = null;
            if (moduleAttribute != null)
            {
                GetOrCreateModule(typeSymbol, typeSymbol.Name, moduleAttribute, out moduleToGenerate);
            }

            foreach (var member in typeSymbol.GetMembers())
            {
                var attr = member.GetAttributes().FirstOrDefault(x => x.AttributeClass.Name == nameof(ActionMenuMemberAttribute));
                if (attr == null) continue;

                var name = attr.ConstructorArguments[0].Value?.ToString();
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new Exception("Name cannot be null or empty.");
                }

                var containingType = member.ContainingSymbol;
                var className = containingType.Name;

                // In case the module is built-in, we still generate a module for it
                if (moduleToGenerate == null)
                {
                    GetOrCreateModule(typeSymbol, className, moduleAttribute!, out moduleToGenerate);
                }

                var method = member as IMethodSymbol;
                var desc = new KalkMemberToGenerate()
                    {
                        Name = name,
                        XmlId = member.GetDocumentationCommentId(),
                        Category = string.Empty,
                        IsCommand = method != null && method.ReturnsVoid,
                        Module = moduleToGenerate,
                    };
                desc.Names.Add(name);
                    
                if (method != null)
                {
                    desc.CSharpName = method.Name;
                        
                    var builder = new StringBuilder();
                    desc.IsAction = method.ReturnsVoid;
                    desc.IsFunc = !desc.IsAction;
                    builder.Append(desc.IsAction ? "Action" : "Func");

                    if (method.Parameters.Length > 0 || desc.IsFunc)
                    {
                        builder.Append('<');
                    }

                    for (var i = 0; i < method.Parameters.Length; i++)
                    {
                        var parameter = method.Parameters[i];
                        if (i > 0) builder.Append(", ");
                        builder.Append(GetTypeName(parameter.Type));
                    }

                    if (desc.IsFunc)
                    {
                        if (method.Parameters.Length > 0)
                        {
                            builder.Append(", ");
                        }
                        builder.Append(GetTypeName(method.ReturnType));
                    }

                    if (method.Parameters.Length > 0 || desc.IsFunc)
                    {
                        builder.Append('>');
                    }

                    desc.Cast = $"({builder})";
                }
                else if (member is IPropertySymbol or IFieldSymbol)
                {
                    desc.CSharpName = member.Name;
                    desc.IsConst = true;
                }

                moduleToGenerate.Members.Add(desc);
                ExtractDocumentation(member, desc);
            }
        }

        var modules = mapNameToModule.Values.OrderBy(x => x.ClassName).ToList();
        var templateStr = await File.ReadAllTextAsync("Templates/Module.scriban-cs");
        var template = Template.Parse(templateStr);

        var context = new TemplateContext
            {
                LoopLimit = 0,
                MemberRenamer = x => x.Name
            };
        var scriptObject = new ScriptObject()
            { 
                { "modules", modules },
            };
        context.PushGlobal(scriptObject);

        var result = await template.RenderAsync(context);
        await File.WriteAllTextAsync(pathToGeneratedCode, result);
        // await File.WriteAllTextAsync(Path.Combine(srcFolder, "ScribanRepoM.Tests", "RepoM.ActionMenu", "Generated", "Coen.generated.cs"), result);

        
        // Generate module site documentation
        foreach(KalkModuleToGenerate module in modules)
        {
            await GenerateModuleSiteDocumentation(module, docsFolder);
        }

        return;

        // Log any errors if a member doesn't have any doc or tests
        var functionWithMissingDoc = 0;
        var functionWithMissingTests = 0;
        foreach (var module in modules)
        {
            foreach (var member in module.Members)
            {
                var hasNoDesc = string.IsNullOrEmpty(member.Description);
                var hasNoTests = member.Tests.Count == 0;
                if ((!hasNoDesc && !hasNoTests) || module.ClassName.Contains("Intrinsics"))
                {
                    continue;
                }

                // We don't log for all the matrix constructors, as they are tested separately.
                if (module.ClassName == "TypesModule" && member.CSharpName.StartsWith("Create"))
                {
                    continue;
                }

                if (hasNoDesc)
                {
                    ++functionWithMissingDoc;
                }

                if (hasNoTests)
                {
                    ++functionWithMissingTests;
                }

                Console.WriteLine($"The member {member.Name} => {module.ClassName}.{member.CSharpName} doesn't have {(hasNoTests ? "any tests" + (hasNoDesc ? " and" : "") : "")} {(hasNoDesc ? "any docs" : "")}");
            }
        }

        Console.WriteLine($"{modules.Count} modules generated.");
        Console.WriteLine($"{modules.SelectMany(x => x.Members).Count()} functions generated.");
        Console.WriteLine($"{modules.SelectMany(x => x.Members).SelectMany(y => y.Tests).Count()} tests generated.");
        Console.WriteLine($"{functionWithMissingDoc} functions with missing doc.");
        Console.WriteLine($"{functionWithMissingTests} functions with missing tests.");
    }


    private static async Task GenerateModuleSiteDocumentation(KalkModuleToGenerate module, string siteFolder)
    {
        if (module.Name == "KalkEngine")
        {
            module.Name = "General";
        }
            
        module.Members.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.Ordinal));

        module.Title = $"{module.Name} {(module.IsBuiltin ? "Functions":"Module")}";

        var name = module.Name.ToLowerInvariant();
        module.Url = $"/doc/api/{name}/";

        const string templateText = @"---
title: {{module.Title}}
url: {{module.Url}}
---
{{~ if !module.IsBuiltin ~}}

{{ module.Description }}

In order to use the functions provided by this module, you need to import this module:

```kalk
>>> import {{module.Name}}
```
{{~ end ~}}
{{~ if (module.Title | string.contains 'Intrinsics') ~}}

In order to use the functions provided by this module, you need to import this module:

```kalk
>>> import HardwareIntrinsics
```
{%{~
{{NOTE do}}
~}%}
These intrinsic functions are only available if your CPU supports `{{module.Name}}` features.
{%{~
{{end}}
~}%}

{{~ end ~}}
{{~ for member in module.Members ~}}

## {{member.Name}}

`{{member.Name}}{{~ if member.Params.size > 0 ~}}({{~ for param in member.Params ~}}{{ param.Name }}{{ param.IsOptional?'?':''}}{{ for.last?'':',' }}{{~ end ~}}){{~ end ~}}`

{{~ if member.Description ~}}
{{ member.Description | regex.replace `^\s{4}` '' 'm' | string.rstrip }}
{{~ end ~}}
{{~ if member.Params.size > 0 ~}}

    {{~ for param in member.Params ~}}
- `{{ param.Name }}`: {{ param.Description}}
    {{~end ~}}
{{~ end ~}}
{{~ if member.Returns ~}}

### Returns

{{ member.Returns | regex.replace `^\s{4}` '' 'm' | string.rstrip }}
{{~ end ~}}
{{~ if member.Remarks ~}}

### Remarks

{{ member.Remarks | regex.replace `^\s{4}` '' 'm' | string.rstrip }}
{{~ end ~}}
{{~ if member.Example ~}}

### Example

```kalk
{{ member.Example | regex.replace `^\s{4}` '' 'm' | string.rstrip }}
```
{{~ end ~}}
{{~ end ~}}
";
        var template = Template.Parse(templateText);

        var apiFolder = siteFolder;

        //
        // // Don't generate hardware.generated.md
        // if (name == "hardware")
        // {
        //     return;
        // }

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
        var result = await template.RenderAsync(context);

        await File.WriteAllTextAsync(Path.Combine(apiFolder, $"{name}.generated.md"), result);
    }

    private static (string, string)? TryParseTest(string text)
    {
        var testLines = new StringReader(text);
        string? line;
        string input = null;
        var output = string.Empty;
        var startColumn = -1;
        while ((line = testLines.ReadLine()) != null)
        {
            line = line.TrimEnd();
            var matchPrompt = _promptRegex.Match(line);
            if (matchPrompt.Success)
            {
                startColumn = matchPrompt.Groups[1].Length;
                input += line.Substring(matchPrompt.Length) + Environment.NewLine;
            }
            else
            {
                if (startColumn < 0)
                {
                    throw new InvalidOperationException($"Expecting a previous prompt line >>> before `{line}`");
                }

                line = line.Length >= startColumn ? line[startColumn..] : line;
                // If we have a result with ellipsis `...` we can't test this text.
                if (line.StartsWith("..."))
                {
                    return null;
                }
                output += line + Environment.NewLine;
            }
        }

        return input != null ? (input.TrimEnd(), output.TrimEnd()) : null;
    }

    private static void ExtractDocumentation(ISymbol symbol, KalkDescriptorToGenerate desc)
    {
        var xmlStr = symbol.GetDocumentationCommentXml();
        if (xmlStr.Contains("Find files in a given directory based on the search pattern. Resulting filenames are absolute path based."))
        {
            xmlStr = xmlStr;
        }
        try
        {
            if (!string.IsNullOrEmpty(xmlStr))
            {
                var xmlDoc = XElement.Parse(xmlStr);
                var elements = xmlDoc.Elements().ToList();

                foreach (var element in elements)
                {
                    var text = GetCleanedString(element).Trim();
                    if (element.Name == "summary")
                    {
                        desc.Description = text;
                    }
                    else if (element.Name == "param")
                    {
                        var argName = element.Attribute("name")?.Value;
                        if (argName != null && symbol is IMethodSymbol method)
                        {
                            var parameterSymbol = method.Parameters.FirstOrDefault(x => x.Name == argName);
                            var isOptional = false;
                            if (parameterSymbol == null)
                            {
                                Console.WriteLine($"Invalid XML doc parameter name {argName} not found on method {method}");
                            }
                            else
                            {
                                isOptional = parameterSymbol.IsOptional;
                            }

                            desc.Params.Add(new KalkParamDescriptor(argName, text) { IsOptional = isOptional, });
                        }
                    }
                    else if (element.Name == "returns")
                    {
                        desc.Returns = text;
                    }
                    else if (element.Name == "remarks")
                    {
                        desc.Remarks = text;
                    }
                    else if (element.Name == "example")
                    {
                        text = _removeCode.Replace(text, string.Empty);
                        desc.Example += text;
                        // var test = TryParseTest(text);
                        // if (test != null)
                        // {
                        //     desc.Tests.Add(test.Value);
                        // }
                    }
                    else if (element.Name == "test")
                    {
                        text = _removeCode.Replace(text, string.Empty);
                        // var test = TryParseTest(text);
                        // if (test != null)
                        // {
                        //     desc.Tests.Add(test.Value);
                        // }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error while processing `{symbol}` with XML doc `{xmlStr}", ex);
        }
    }


    static string GetTypeName(ITypeSymbol typeSymbol)
    {
        //if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
        //{
        //    return GetTypeName(arrayTypeSymbol.ElementType) + "[]";
        //}

        //if (typeSymbol.Name == typeof(Nullable).Name)
        //{
        //    return typeSymbol.ToDisplayString();
        //}
           
        //if (typeSymbol.Name == "String") return "string";
        //if (typeSymbol.Name == "Object") return "object";
        //if (typeSymbol.Name == "Boolean") return "bool";
        //if (typeSymbol.Name == "Single") return "float";
        //if (typeSymbol.Name == "Double") return "double";
        //if (typeSymbol.Name == "Int32") return "int";
        //if (typeSymbol.Name == "Int64") return "long";
        return typeSymbol.ToDisplayString();
    }

    private static string GetCleanedString(XNode node)
    {
        if (node.NodeType == XmlNodeType.Text)
        {
            return node.ToString();
        }

        var element = (XElement) node;
        string text;
        if (element.Name == "paramref")
        {
            text = element.Attribute("name")?.Value ?? string.Empty;
        }
        else
        {

            var builder = new StringBuilder();
            foreach (var subElement in element.Nodes())
            {
                builder.Append(GetCleanedString(subElement));
            }

            text = builder.ToString();
        }

        if (element.Name == "para")
        {
            text += "\n";
        }
        return HttpUtility.HtmlDecode(text);
    }

    [GeneratedRegex("^\\s*```\\w*[ \\t]*[\\r\\n]*", RegexOptions.Multiline)]
    private static partial Regex RemoveCodeRegex();

    [GeneratedRegex("^(\\s*)>>>\\s")]
    private static partial Regex PromptRegex();
}