namespace RepoM.ActionMenu.CodeGen.Misc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using RepoM.ActionMenu.CodeGen.Models;

internal static partial class XmlDocsParser
{
    public static void ExtractDocumentation(ISymbol symbol, IXmlDocsExtended desc, IDictionary<string, string> files)
    {
        var xmlStr = symbol.GetDocumentationCommentXml();
        ExtractDocumentation(xmlStr, symbol, desc, files);
    }

    internal static void ExtractDocumentation(string? xmlStr, ISymbol symbol, IXmlDocsExtended desc, IDictionary<string, string> files)
    {
        if (string.IsNullOrEmpty(xmlStr))
        {
            return;
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
                    desc.Description = SanitizeMultilineText(text);
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

                    desc.Params.Add(new ParamDescriptor(argName, text) { IsOptional = isOptional, });
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
                    ExamplesDescriptor examplesDescriptor = GetExampleData(element, files);
                    desc.Examples = examplesDescriptor;
                }
                else if (element.Name == "test")
                {
                    // todo?
                    _ = _removeCode.Replace(text, string.Empty);
                }
                else if (element.Name == "inheritdoc")
                {
                    // expect text to be empty
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        throw new Exception("Text should be empty in inheritdoc");
                    }

                    // we need cref
                    var cref = element.Attribute("cref")?.Value;

                    if (string.IsNullOrWhiteSpace(cref))
                    {
                        throw new Exception("Cref should not be empty in inheritdoc");
                    }

                    if (!cref.StartsWith("P:") && !cref.StartsWith("T:"))
                    {
                        throw new Exception("Cref should start with P: or T:");
                    }

                    desc.InheritDocs = cref[2..];
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error while processing `{symbol}` with XML doc `{xmlStr}", ex);
        }
    }

    private static string SanitizeMultilineText(string text)
    {
        return text.Replace("\n    ", "\n");
    }

    public static ExamplesDescriptor GetExampleData(XNode node, IDictionary<string, string> files)
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

        foreach (XNode item in nodes)
        {
            if (item is XText xText)
            {
                result.Items.Add(new Text() { Content = xText.Value.Trim(), });

            }
            else if (item is XElement xElement)
            {
                if (xElement.Name == "para")
                {
                    result.Items.Add(new Paragraph { Text = xElement.Value.Trim(), });
                }
                else if (xElement.Name == "usage")
                {
                    result.Items.Add(new Header { Text = "Usage", });
                }
                else if (xElement.Name == "result")
                {
                    result.Items.Add(new Header { Text = "Result", });
                }
                else if (xElement.Name == "repository-action-sample")
                {
                    result.Items.Add(new Header { Text = "RepositoryAction sample", });
                }
                else if (xElement.Name == "code")
                {
                    result.Items.Add(new Code() { Content = xElement.Value.Trim(), Language = null, UseRaw = false, });
                }
                else if (xElement.Name == "snippet")
                {
                    // markdown snippet
                    XAttribute? customAttribute = xElement.Attributes().SingleOrDefault(x => x.Name == "name");
                    if (customAttribute == null)
                    {
                        throw new Exception("name attribute should exist");
                    }
                    var name = customAttribute.Value.Trim();

                    if (string.IsNullOrEmpty(name))
                    {
                        throw new Exception("Name should not be null");
                    }

                    var snippet = new Snippet { Name = name, };

                    customAttribute = xElement.Attributes().SingleOrDefault(x => x.Name == "mode");
                    if (customAttribute != null)
                    {
                        if (Enum.TryParse(customAttribute.Value.Trim().AsSpan(), true, out SnippetMode mode))
                        {
                            snippet.Mode = mode;
                        }
                        else
                        {
                            throw new Exception("mode attribute should not be empty");
                        }
                    }

                    customAttribute = xElement.Attributes().SingleOrDefault(x => x.Name == "language");
                    if (customAttribute != null)
                    {
                        if (!string.IsNullOrWhiteSpace(customAttribute.Value))
                        {
                            snippet.Language = customAttribute.Value.Trim();
                        }
                        else
                        {
                            throw new Exception("language attribute should not be empty");
                        }
                    }

                    result.Items.Add(snippet);
                }
                else if (xElement.Name == "code-file")
                {
                    XAttribute? customAttribute = xElement.Attributes().SingleOrDefault(x => x.Name == "filename");
                    if (customAttribute == null)
                    {
                        throw new Exception("filename attribute should exist");
                    }
                    var filename = customAttribute.Value.Trim();

                    if (!files.TryGetValue(filename, out var content))
                    {
                        throw new Exception($"File '{filename}' not found");
                    }

                    var code = new Code { Content = content, UseRaw = true, };

                    customAttribute = xElement.Attributes().SingleOrDefault(x => x.Name == "language");
                    if (customAttribute != null)
                    {
                        if (!string.IsNullOrWhiteSpace(customAttribute.Value))
                        {
                            code.Language = customAttribute.Value.Trim();
                        }
                        else
                        {
                            throw new Exception("language attribute should not be empty");
                        }
                    }

                    // check if file exists, load sample
                    result.Items.Add(code);
                }
                else if (xElement.Name == "md-snippet")
                {
                    Console.WriteLine("WARNING md-snippet");
                    var snippetName = xElement.Value.Trim();
                    result.Items.Add(new Text() { Content = "snippet: " + snippetName, });
                }
                else if (xElement.Name == "md-include")
                {
                    Console.WriteLine("WARNING md-include");
                    var includeName = xElement.Value.Trim();
                    result.Items.Add(new Text() { Content = "include: " + includeName, });
                }
                else
                {
                    throw new Exception($"'{xElement.Name}' Not expected");
                }
            }
        }

        return result;
    }

    public static string GetCleanedString(XNode node)
    {
        if (node.NodeType == XmlNodeType.Text)
        {
            // return node.ToString();
            var s = node.ToString();
            return HttpUtility.HtmlDecode(s);
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