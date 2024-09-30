namespace RepoM.ActionMenu.Core.Tests.Yaml.Model;

using System;
using System.Threading.Tasks;
using FluentAssertions;
using RepoM.ActionMenu.Core.Misc;
using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using Scriban;
using Xunit;

public class ScribanPredicateTests
{
    [Fact]
    public async Task EvaluateAsync_ShouldThrowPredicateEvaluationException_WhenPredicateIsWrong()
    {
        // arrange
        var sut = new ScribanPredicate
            {
                Value = "file.exists x",
            };
        var realTemplateParser = new FixedTemplateParser();
        ((ICreateTemplate)sut).CreateTemplate(realTemplateParser);
        ITemplateEvaluator templateEvaluator = new FakeTemplateContext(realTemplateParser);

        // act
        Func<Task<bool>> act = async () => await sut.EvaluateAsync(templateEvaluator);

        // assert
        (await act.Should().ThrowAsync<PredicateEvaluationException>())
            .WithMessage("Could not evaluate predicate 'file.exists x' because <input>(1,6) : error : Cannot get the member file.exists for a null object.")
            .And.PredicateText.Should().Be("file.exists x");

    }
}

file class FakeTemplateContext : TemplateContext, ITemplateEvaluator
{
    private readonly ITemplateParser _templateParser;

    public FakeTemplateContext(ITemplateParser templateParser)
    {
        _templateParser = templateParser;
    }
    public Task<string> RenderStringAsync(string text)
    {
        throw new NotImplementedException();
    }

    public async Task<object> EvaluateAsync(string text)
    {
        Template template = _templateParser.ParseScriptOnly(text);
        return await template.EvaluateAsync(this).ConfigureAwait(false);
    }
}