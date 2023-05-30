namespace RepoM.Plugin.Clipboard.Tests.ActionProvider;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using RepoM.Api.Common;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoM.Core.Plugin.Expressions;
using RepoM.Plugin.Clipboard.ActionProvider;
using VerifyTests;
using VerifyXunit;
using Xunit;
using Repository = RepoM.Api.Git.Repository;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

[UsesVerify]
public class ActionClipboardCopyV1MapperTests
{
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator;
    private readonly Repository _repository;
    private readonly ActionMapperComposition _actionMapperComposition;
    private readonly VerifySettings _verifySettings;
    private readonly ActionClipboardCopyV1Mapper _sut;
    private readonly RepositoryActionClipboardCopyV1 _action;

    public ActionClipboardCopyV1MapperTests()
    {
        _verifySettings = new VerifySettings();
        _verifySettings.UseDirectory("Verified");

        _actionMapperComposition = new ActionMapperComposition(new List<IActionToRepositoryActionMapper>(), A.Dummy<IRepositoryExpressionEvaluator>());
        _repository = new Repository("dummy");
        _expressionEvaluator = A.Fake<IRepositoryExpressionEvaluator>();
        ITranslationService translationService = A.Fake<ITranslationService>();

        _sut = new ActionClipboardCopyV1Mapper(
            _expressionEvaluator,
            translationService);

        _action = new RepositoryActionClipboardCopyV1
            {
                Active = "true",
                Name = "Abc",
                Text = "text123",
                MultiSelectEnabled = "false",
            };
        A.CallTo(() => _expressionEvaluator.EvaluateBooleanExpression("true", _repository)).Returns(true);
        A.CallTo(() => _expressionEvaluator.EvaluateBooleanExpression("false", _repository)).Returns(false);
        A.CallTo(() => _expressionEvaluator.EvaluateStringExpression(A<string>._, _repository))
         .ReturnsLazily(call =>
             {
                 if (call.Arguments[0] is string s)
                 {
                     return "ES_" + s;
                 }

                 throw new Exception("Thrown by test, Not expected");
             });
        A.CallTo(() => translationService.Translate(A<string>._)).ReturnsLazily(call => call.Arguments[0] as string ?? "unexpected by test.");
        A.CallTo(() => translationService.Translate(A<string>._, A<object[]>._)).ReturnsLazily(call => call.Arguments[0] as string ?? "unexpected by test.");
    }

    [Fact]
    public void Ctor_ShouldThrowArgumentNullException_WhenAnArgumentIsNull()
    {
        // arrange
        IRepositoryExpressionEvaluator expressionEvaluator = A.Dummy<IRepositoryExpressionEvaluator>();
        ITranslationService translationService = A.Dummy<ITranslationService>();

        // act
        var actions = new List<Action>
            {
                () => _ = new ActionClipboardCopyV1Mapper(expressionEvaluator, null!),
                () => _ = new ActionClipboardCopyV1Mapper(null!, translationService),
            };

        // assert
        foreach (Action action in actions)
        {
            action.Should().Throw<ArgumentNullException>();
        }
    }

    [Fact]
    public void CanHandleMultipleRepositories_ShouldBeFalse()
    {
        // arrange

        // act
        var result = _sut.CanHandleMultipleRepositories();

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanMap_ShouldReturnFalse_WhenWrongType()
    {
        // arrange

        // act
        var result = _sut.CanMap(new RepositoryAction());

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanMap_ShouldReturnFalse_WhenNull()
    {
        // arrange
        RepositoryActionClipboardCopyV1 action = null!;

        // act
        var result = _sut.CanMap(action);

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanMap_ShouldReturnTrue_WhenRepositoryActionClipboardCopyV1()
    {
        // arrange
        var action = new RepositoryActionClipboardCopyV1();

        // act
        var result = _sut.CanMap(action);

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Map_ShouldReturnEmpty_WhenActionIsNull()
    {
        // arrange

        // act
        var result = _sut.Map(null!, new []{ _repository, }, _actionMapperComposition).ToArray();

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Map_ShouldReturnEmpty_WhenActionActiveIsFalse()
    {
        // arrange
        _action.Active = "false";

        // act
        var result = _sut.Map(_action, new []{ _repository, }, _actionMapperComposition).ToArray();

        // assert
        result.Should().BeEmpty();
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("text12313")]
    [InlineData("a b c2")]
    public async Task Map_ShouldMap_WhenTextSet(string? text)
    {
        // arrange
        _action.Name = "name123";
        _action.Text = text;

        // act
        var result = _sut.Map(_action, new[] { _repository, }, _actionMapperComposition).ToArray();

        // assert
        var paramText = text ?? "null";
        if (paramText == "")
        {
            paramText = "empty";
        }

        await Verifier.Verify(result, _verifySettings).UseTextForParameters(paramText);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("text12313")]
    [InlineData("a b c2")]
    public async Task Map_ShouldMap_WhenNameSet(string? name)
    {
        // arrange
        _action.Name = name;
        
        // act
        var result = _sut.Map(_action, new[] { _repository, }, _actionMapperComposition).ToArray();

        // assert
        var paramText = name ?? "null";
        if (paramText == "")
        {
            paramText = "empty";
        }

        await Verifier.Verify(result, _verifySettings).UseTextForParameters(paramText);
    }
}