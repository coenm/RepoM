namespace RepoM.Plugin.Misc.Tests.TestFramework.NuDoc;

using System;
using System.IO;
using System.Reflection;
using NuDoq;
using RepoM.Api.Common;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

internal class AppSettingsMarkdownVisitor : Visitor
{
    private readonly Type _repositoryActionType;
    private StringWriter _writer;
    private StringWriter _writerSummary;

    public AppSettingsMarkdownVisitor()
    {
        _repositoryActionType = typeof(AppSettings);
        ClassWriter = new ClassWriter();
        _writerSummary = new StringWriter();
        _writer = new StringWriter();
    }

    public ClassWriter ClassWriter { get; }

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
            _writer = ClassWriter.Head;

            base.VisitMember(member);

            _writer = ClassWriter.Head;

            _writer.WriteLine(_writerSummary);
            _writer.WriteLine();
        }
        else if (member.Info is PropertyInfo propertyInfo && IsBuiltinType(propertyInfo.DeclaringType))
        {
            if (propertyInfo.IsObsolete())
            {
                // skip because it obsolete
                base.VisitMember(member);
            }
            else
            {
                var propertyName = propertyInfo.Name;

                _writer = ClassWriter.Properties;

                base.VisitMember(member);

                _writer = ClassWriter.Properties;

                var propertyAttributes = propertyInfo.PropertyAttributesToString();

                var summary = _writerSummary.ToString();
                _writer.WriteLine(string.IsNullOrWhiteSpace(summary)
                    ? $"- `{propertyName}` (no description known){propertyAttributes}"
                    : $"- `{propertyName}`: {summary}{propertyAttributes}");
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