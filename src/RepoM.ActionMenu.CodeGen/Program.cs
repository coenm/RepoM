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
using Microsoft.CodeAnalysis;
using RepoM.ActionMenu.CodeGen.Models;
using RepoM.ActionMenu.Interface.Attributes;
using Scriban;
using Scriban.Runtime;

public partial class Program
{
    private static readonly Regex _removeCode = RemoveCodeRegex();
    
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

        CheckDirectory(Path.Combine(rootFolder, ".git"));
        CheckDirectory(srcFolder);
        CheckDirectory(docsFolder);
        CheckFile(pathToSolution);

        Template templateModule = await LoadTemplateAsync("Templates/Module.scriban-cs");
        Template templateDocs = await LoadTemplateAsync("Templates/Docs.scriban-txt");
        
        CSharpCompilationCaptureResult compilationCaptureResult = CSharpCompilationCapture.Build(pathToSolution);
        Solution solution = compilationCaptureResult.Workspace.CurrentSolution;
        Project[] solutionProjects = solution.Projects.ToArray();
        Project project = Array.Find(solutionProjects, x => x.Name == projectName) ?? throw new Exception($"Project `{projectName}` not found in solution");

        // Make sure that doc will be parsed
        project = project.WithParseOptions(project.ParseOptions!.WithDocumentationMode(DocumentationMode.Parse));

        // Compile the project
        Compilation compilation = await project.GetCompilationAsync() ?? throw new Exception("Compilation failed");
        ValidateCompilation(compilation);

        // _ = compilation.GetTypeByMetadataName("Kalk.Core.KalkEngine");
        var mapNameToModule = new Dictionary<string, KalkModuleToGenerate>();

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
                GetOrCreateModule(typeSymbol, typeSymbol.Name, moduleAttribute, out moduleToGenerate, mapNameToModule);
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
                    GetOrCreateModule(typeSymbol, className, moduleAttribute!, out moduleToGenerate, mapNameToModule);
                }

                var method = member as IMethodSymbol;
                var desc = new KalkMemberToGenerate()
                    {
                        Name = name,
                        XmlId = member.GetDocumentationCommentId(),
                        Category = string.Empty,
                        IsCommand = method?.ReturnsVoid ?? false,
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
                        if (i > 0)
                        {
                            builder.Append(", ");
                        }

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

        var context = new TemplateContext
            {
                LoopLimit = 0,
                MemberRenamer = x => x.Name,
            };
        var scriptObject = new ScriptObject()
            { 
                { "modules", modules },
            };
        context.PushGlobal(scriptObject);

        var result = await templateModule.RenderAsync(context);
        await File.WriteAllTextAsync(pathToGeneratedCode, result);

        
        // Generate module site documentation
        foreach(KalkModuleToGenerate module in modules)
        {
            await GenerateModuleSiteDocumentation(module, docsFolder, templateDocs);
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
                if ((!hasNoDesc))
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

                Console.WriteLine($"The member {member.Name} => {module.ClassName}.{member.CSharpName} doesn't have {(hasNoDesc ? "any docs" : "")}");
            }
        }

        Console.WriteLine($"{modules.Count} modules generated.");
        Console.WriteLine($"{modules.SelectMany(x => x.Members).Count()} functions generated.");
        Console.WriteLine($"{functionWithMissingDoc} functions with missing doc.");
        Console.WriteLine($"{functionWithMissingTests} functions with missing tests.");
    }

    private static void GetOrCreateModule(ITypeSymbol typeSymbol, string className, AttributeData moduleAttribute, out KalkModuleToGenerate moduleToGenerate, Dictionary<string, KalkModuleToGenerate>? mapNameToModule)
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

    private static void ValidateCompilation(Compilation compilation)
    {
        ImmutableArray<Diagnostic> diagnostics = compilation.GetDiagnostics();
        Diagnostic[] errors = diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();

        if (errors.Length <= 0)
        {
            return;
        }

        Console.WriteLine("Compilation errors:");
        foreach (Diagnostic error in errors)
        {
            Console.WriteLine(error);
        }

        Console.WriteLine("Error, Exiting.");
        Environment.Exit(1);
        throw new Exception("Compilation error");
    }

    private static async Task GenerateModuleSiteDocumentation(KalkModuleToGenerate module, string siteFolder, Template templateDocs)
    {
        if (module.Name == "KalkEngine")
        {
            module.Name = "General";
        }
            
        module.Members.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.Ordinal));

        module.Title = $"{module.Name} {(module.IsBuiltin ? "Functions":"Module")}";

        var name = module.Name.ToLowerInvariant();
        module.Url = $"/doc/api/{name}/";

        var apiFolder = siteFolder;

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
        var result = await templateDocs.RenderAsync(context);

        await File.WriteAllTextAsync(Path.Combine(apiFolder, $"{name}.generated.md"), result);
    }

    private static void ExtractDocumentation(ISymbol symbol, KalkDescriptorToGenerate desc)
    {
        var xmlStr = symbol.GetDocumentationCommentXml();
        if (xmlStr.Contains("Returns an enumerable collection of full paths of the files or directories that matches the specified search pattern."))
        {
            xmlStr = xmlStr;
        }

        if (xmlStr.Contains("Checks if the specified file path exists on the disk."))
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
                        ExamplesDescriptor examplesDescriptor = GetExampleData(element);
                        desc.Examples.Add(examplesDescriptor);
                    }
                    else if (element.Name == "test")
                    {
                        _ = _removeCode.Replace(text, string.Empty);
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

    private static ExamplesDescriptor GetExampleData(XNode node)
    {
        var result = new ExamplesDescriptor();

        if (node.NodeType != XmlNodeType.Element)
        {
            return result;
            // expect example node
        }

        var element = (XElement)node;
        if (element.Name != "example")
        {
            return result;
            // expect example node
        }

        XNode[] nodes = element.Nodes().ToArray();
        if (nodes.Length == 0)
        {
            result.Description = element.Value.Trim();
            return result;
        }

        string current = "";

        foreach (var item in nodes)
        {
            if (item is XText xtext)
            {
                current += xtext.Value.Trim() + Environment.NewLine;
            }
            else if (item is XElement xelement)
            {
                if (xelement.Name == "code")
                {
                    if (result.Input != null)
                    {
                        // switch next
                        result.Output = xelement.Value.Trim();
                    }
                    else       
                    {
                        // switch next
                        result.Description = current;
                        result.Input = xelement.Value.Trim();
                    }
                }
                else if (xelement.Name == "para")
                {
                    current += xelement.Value.Trim() + Environment.NewLine;
                }
                else
                {
                    throw new Exception($"'{xelement.Name}' Not expected");
                }
            }       
        }

        if (string.IsNullOrEmpty(result.Description))
        {
            result.Description = current;
        }

        return result;
    }

    private static void CheckDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            throw new Exception($"Folder '{path}' does not exist");
        }
    }

    private static void CheckFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new Exception($"File '{path}' does not exist");
        }
    }

    private static async Task<Template> LoadTemplateAsync(string path)
    {
        var rawTemplate = await File.ReadAllTextAsync(path);
        var template = Template.Parse(rawTemplate);
        if (template.HasErrors)
        {
            throw new Exception(template.Messages.ToString());
        }

        return template;
    }

    [GeneratedRegex("^\\s*```\\w*[ \\t]*[\\r\\n]*", RegexOptions.Multiline)]
    private static partial Regex RemoveCodeRegex();
}