namespace RepoM.Plugin.Misc.Tests.TestFramework.NuDoc;

using NuDoq;
using System.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

internal class RepositoryActionBaseMarkdownVisitor : Visitor
{
    private readonly Type _repositoryActionType;
    private StringWriter _writer;
    private StringWriter _writerSummary;

    public RepositoryActionBaseMarkdownVisitor(Type repositoryActionType)
    {
        _repositoryActionType = repositoryActionType;
        classWriter = new ClassWriter();
        _writerSummary = new StringWriter();
        _writer = new StringWriter();
    }

    public ClassWriter classWriter { get; }

    private bool IsBuiltinType(Type? type)
    {
        if (type == null)
        {
            return false;
        }

        return type == _repositoryActionType;
    }
    
    public override void VisitMember(Member member)
    {
        if (member.Info is Type type && IsBuiltinType(type))
        {
            _writer = classWriter.Head;

            base.VisitMember(member);

            _writer = classWriter.Head;

            _writer.WriteLine(_writerSummary);
            _writer.WriteLine();
        }
        else if (member.Info is PropertyInfo propertyInfo && IsBuiltinType(propertyInfo.DeclaringType))
        {
           

            if (propertyInfo.Name.Equals(nameof(RepositoryAction.MultiSelectEnabled)))
            {
                // skip because it is sort of obsolete
                base.VisitMember(member);
            }
            else
            {
                var methodShortName = propertyInfo.SanitizePropertyName();

                _writer = classWriter.Properties;

                base.VisitMember(member);

                _writer = classWriter.Properties;

                var propertyAttributes = propertyInfo.PropertyAttributesToString();

                var summary = _writerSummary.ToString();
                _writer.WriteLine(string.IsNullOrWhiteSpace(summary)
                    ? $"- `{methodShortName}` (no description known){propertyAttributes}"
                    : $"- `{methodShortName}`: {summary}{propertyAttributes}");
            }
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

internal class RepositoryActionMarkdownVisitor : Visitor
{
    private readonly Dictionary<string, string> _builtinClassNames;
    private StringWriter _writer;
    private StringWriter _writerSummary;

    public RepositoryActionMarkdownVisitor(Dictionary<string, string> builtinClassNames)
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
            var methodShortName = propertyInfo.SanitizePropertyName();

            ClassWriter classWriter = ClassWriters[shortName];

            _writer = classWriter.Properties;

            base.VisitMember(member);

            _writer = classWriter.Properties;

            var propertyAttributes = propertyInfo.PropertyAttributesToString();

            var summary = _writerSummary.ToString();

            _writer.WriteLine(string.IsNullOrWhiteSpace(summary)
                ? $"- `{methodShortName}` (no description known){propertyAttributes}"
                : $"- `{methodShortName}`: {summary}{propertyAttributes}");
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