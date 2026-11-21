namespace RepoM.StartupPlugin.Demo.Tests.TestFramework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using VerifyTests;

internal static class OrderInsensitiveXmlComparer
{
    public static Task<CompareResult> CompareAsync(
        string received,
        string verified,
        IReadOnlyDictionary<string, object> context)
    {
        var receivedDocument = Normalize(received);
        var verifiedDocument = Normalize(verified);

        return Task.FromResult(
            XNode.DeepEquals(receivedDocument, verifiedDocument)
                ? new CompareResult(true)
                : CompareResult.NotEqual("XML documents differ after sorting elements and attributes."));
    }

    internal static XDocument Normalize(string xml)
    {
        var document = XDocument.Parse(xml);
        Sort(document.Root!);
        return document;
    }

    private static void Sort(XElement element)
    {
        if (element.Document?.Root == element && element.Name.LocalName == "WorktreePreviewWindow")
        {
            element.Attribute("Top")?.SetValue("{WindowPosition}");
            element.Attribute("Left")?.SetValue("{WindowPosition}");
        }

        foreach (var child in element.Elements().ToArray())
        {
            Sort(child);
        }

        var attributes = element.Attributes()
            .OrderBy(GetSortName, StringComparer.Ordinal)
            .ToArray();

        foreach (var attribute in attributes)
        {
            attribute.Remove();
        }

        element.Add(attributes);

        var childElements = element.Elements()
            .OrderBy(GetSortName, StringComparer.Ordinal)
            .ThenBy(child => child.ToString(SaveOptions.DisableFormatting), StringComparer.Ordinal)
            .ToArray();
        var nodes = element.Nodes().ToArray();
        foreach (var node in nodes)
        {
            node.Remove();
        }

        var childIndex = 0;
        foreach (var node in nodes)
        {
            element.Add(node is XElement ? childElements[childIndex++] : node);
        }
    }

    private static string GetSortName(XElement element)
    {
        var prefix = element.GetPrefixOfNamespace(element.Name.Namespace);
        return string.IsNullOrEmpty(prefix) ? element.Name.LocalName : $"{prefix}:{element.Name.LocalName}";
    }

    private static string GetSortName(XAttribute attribute)
    {
        if (attribute.IsNamespaceDeclaration)
        {
            return attribute.Name.Namespace == XNamespace.None
                ? "xmlns"
                : $"xmlns:{attribute.Name.LocalName}";
        }

        var prefix = attribute.Parent?.GetPrefixOfNamespace(attribute.Name.Namespace);
        return string.IsNullOrEmpty(prefix) ? attribute.Name.LocalName : $"{prefix}:{attribute.Name.LocalName}";
    }
}
