namespace RepoM.Plugin.Misc.Tests.TestFramework.NuDoc;

using NuDoq;
using System.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

internal class PluginConfigurationMarkdownVisitor : Visitor
{
    private readonly Dictionary<string, string> _builtinClassNames;
    private StringWriter _writer;
    private StringWriter _writerSummary;

    public PluginConfigurationMarkdownVisitor(Dictionary<string, string> builtinClassNames)
    {
        _builtinClassNames = builtinClassNames;

        ClassWriters = new Dictionary<string, ClassWriter>();
        _writerSummary = new StringWriter();
        _writer = new StringWriter();
    }

    public Dictionary<string, ClassWriter> ClassWriters { get; }

    private bool IsBuiltinType(Type? type, [NotNullWhen(true)] out string? shortName)
    {
        shortName = null;
        if (type == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(type.Namespace))
        {
            return false;
        }

        return type.Namespace.StartsWith("RepoM") && _builtinClassNames.TryGetValue(type.Name, out shortName);
    }

    public override void VisitMember(Member member)
    {
        if (member.Info is Type type && IsBuiltinType(type, out var shortName))
        {
            var classWriter = new ClassWriter();
            ClassWriters[shortName] = classWriter;

            _writer = classWriter.Head;

            base.VisitMember(member);

            _writer = classWriter.Head;

            _writer.WriteLine(_writerSummary);
            _writer.WriteLine();
        }
        else if (member.Info is PropertyInfo propertyInfo && IsBuiltinType(propertyInfo.DeclaringType, out shortName))
        {
            var propertyName = propertyInfo.Name;

            ClassWriter classWriter = ClassWriters[shortName];

            _writer = classWriter.Properties;

            base.VisitMember(member);

            _writer = classWriter.Properties;

            var summary = _writerSummary.ToString();
            _writer.WriteLine(string.IsNullOrWhiteSpace(summary)
                ? $"- `{propertyName}` (no description known)"
                : $"- `{propertyName}`: {summary}");
        }

        _writerSummary = new StringWriter();
    }

    public override void VisitSummary(Summary summary)
    {
        _writer = _writerSummary;
        base.VisitSummary(summary);
    }

    public override void VisitText(Text text)
    {
        _writer.Write(text.Content);
    }
}