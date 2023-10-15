namespace RepoM.ActionMenu.CodeGen;

using Microsoft.CodeAnalysis;
using RepoM.ActionMenu.CodeGen.Models;
using System.Xml.Linq;
using System;
using System.Linq;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

internal static partial class XmlDocsParser
{
    public static void ExtractDocumentation(ISymbol symbol, KalkDescriptorToGenerate desc)
    {
        var xmlStr = symbol.GetDocumentationCommentXml();
        ExtractDocumentation(xmlStr, symbol, desc);
    }

    internal static void ExtractDocumentation(string? xmlStr, ISymbol symbol, KalkDescriptorToGenerate desc)
    {
        if (string.IsNullOrEmpty(xmlStr))
        {
            return;
        }

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
            var xmlDoc = XElement.Parse(xmlStr);
            var elements = xmlDoc.Elements().ToList();

            foreach (XElement element in elements)
            {
                var text = GetCleanedString(element).Trim();
                if (element.Name == "summary")
                {
                    desc.Description = text;
                }
                else if (element.Name == "param")
                {
                    var argName = element.Attribute("name")?.Value;
                    if (argName == null || symbol is not IMethodSymbol method)
                    {
                        continue;
                    }

                    IParameterSymbol? parameterSymbol = method.Parameters.FirstOrDefault(x => x.Name == argName);
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
                    // todo?
                    _ = _removeCode.Replace(text, string.Empty);
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error while processing `{symbol}` with XML doc `{xmlStr}", ex);
        }
    }

    public static ExamplesDescriptor GetExampleData(XNode node)
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

    public static string GetCleanedString(XNode node)
    {
        if (node.NodeType == XmlNodeType.Text)
        {
            return node.ToString();
        }

        var element = (XElement)node;
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


    private static readonly Regex _removeCode = RemoveCodeRegex();

    [GeneratedRegex("^\\s*```\\w*[ \\t]*[\\r\\n]*", RegexOptions.Multiline)]
    private static partial Regex RemoveCodeRegex();

}