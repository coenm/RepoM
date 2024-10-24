namespace RepoM.ActionMenu.Core.Tests.Model;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using RepoM.ActionMenu.Core.Misc;
using RepoM.ActionMenu.Core.Model;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.Scriban;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Core.Plugin.Repository;
using Xunit;
using IEnvironment = RepoM.ActionMenu.Core.Abstractions.IEnvironment;

public class DisposableContextScriptObjectTests
{
    private readonly IRepository _repository = A.Fake<IRepository>();
    private readonly ITemplateParser _templateParser = new FixedTemplateParser();
    private readonly IFileSystem _fileSystem = new MockFileSystem();
    private readonly ITemplateContextRegistration[] _functionsArray = [];
    private readonly IActionToRepositoryActionMapper[] _mapper = [];
    private readonly IActionMenuDeserializer _deserializer = A.Fake<IActionMenuDeserializer>();
    private ActionMenuGenerationContext _context;
    private readonly IContextActionProcessor[] _mappers;
    private readonly DisposableContextScriptObject _sut;
    private readonly IEnvironment _environment = A.Fake<IEnvironment>();

    public DisposableContextScriptObjectTests()
    {
        A.CallTo(() => _environment.GetEnvironmentVariables()).Returns(new Dictionary<string, string>()
        {
            { "x", "y" },
        });

        _mappers =
            [
                A.Fake<IContextActionProcessor>(),
                A.Fake<IContextActionProcessor>(),
            ];
        _context = CreateContext();
        _sut = CreateSut();
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<DisposableContextScriptObject> act1 = () => new DisposableContextScriptObject(_context, null!);
        Func<DisposableContextScriptObject> act2 = () => new DisposableContextScriptObject(null!, _mappers);

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task AddContextActionAsync_ShouldNotCallMappers_WhenContextItemIsDisabledContextAction()
    {
        // arrange
        IContextAction contextItem = new EnabledContextAction("dummy", false);

        // act
        await _sut.AddContextActionAsync(contextItem);

        // assert
        foreach (IContextActionProcessor m in _mappers)
        {
            A.CallTo(m).MustNotHaveHappened();
        }
    }

    [Fact]
    public async Task AddContextActionAsync_ShouldThrow_WhenNoMapperFound()
    {
        // arrange
        IContextAction contextItem = A.Fake<IContextAction>();
        A.CallTo(() => _mappers[0].CanProcess(contextItem)).Returns(false);
        A.CallTo(() => _mappers[1].CanProcess(contextItem)).Returns(false);

        // act
        Func<Task> act = async () => await _sut.AddContextActionAsync(contextItem);

        // assert
        _ = await act.Should().ThrowAsync<Exception>().WithMessage("Cannot find mapper");
    }

    [Fact]
    public async Task AddContextActionAsync_ShouldCallMappers_WhenContextItemDoesNotImplementIEnabledInterface()
    {
        // arrange
        IContextAction contextItem = A.Fake<IContextAction>();
        A.CallTo(() => _mappers[0].CanProcess(contextItem)).Returns(false);
        A.CallTo(() => _mappers[1].CanProcess(contextItem)).Returns(false);

        // act
        Func<Task> act = async () => await _sut.AddContextActionAsync(contextItem);

        // assert
        _ = await act.Should().ThrowAsync<Exception>();
        foreach (IContextActionProcessor m in _mappers)
        {
            A.CallTo(() => m.CanProcess(contextItem)).MustHaveHappenedOnceExactly();
            A.CallTo(m).MustHaveHappenedOnceExactly();
        }
    }

    [Fact]
    public async Task AddContextActionAsync_ShouldUseMapperForProcessing_WhenMapperFound()
    {
        // arrange
        IContextAction contextItem = A.Fake<IContextAction>();
        A.CallTo(() => _mappers[0].CanProcess(contextItem)).Returns(true); // this one will be used for processing
        A.CallTo(() => _mappers[1].CanProcess(contextItem)).Returns(false);

        // act
        await _sut.AddContextActionAsync(contextItem);

        // assert
        A.CallTo(() => _mappers[0].ProcessAsync(contextItem, _context, _sut)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _mappers[1].ProcessAsync(A<IContextAction>._, A<IContextMenuActionMenuGenerationContext>._, A<IScope>._)).MustNotHaveHappened();

    }

    [Fact]
    public void PushEnvironmentVariable_ShouldAddVariables()
    {
        // arrange
        A.CallTo(() => _environment.GetEnvironmentVariables())
            .Returns(new Dictionary<string, string>
            {
                { "x", "y" },
            });

        _context = CreateContext();
        IScope sut = CreateSut();

        // assume
        _context.Env.Count.Should().Be(1);

        // act
        sut.PushEnvironmentVariable(
            new Dictionary<string, string>
            {
                { "x1", "y" },
                { "x2", "y" },
            });

        // assert
        _context.Env.Count.Should().Be(3);
        _context.Env.GetMembers().Should().BeEquivalentTo("x", "x1", "x2");
    }

    [Fact]
    public void PushEnvironmentVariable_ShouldAddVariables_Distinct()
    {
        // arrange
        A.CallTo(() => _environment.GetEnvironmentVariables())
         .Returns(new Dictionary<string, string>
             {
                 { "x", "y" },
             });

        _context = CreateContext();
        IScope sut = CreateSut();

        // assume
        _context.Env.Count.Should().Be(1);

        // act
        sut.PushEnvironmentVariable(
            new Dictionary<string, string>
                {
                    { "x1", "y" },
                    { "x", "yNEW" },
                });

        // assert
        _context.Env.Count.Should().Be(2);
        _context.Env.GetMembers().Should().BeEquivalentTo("x", "x1");
    }

    [Fact]
    public void Dispose_ShouldPopAllPushedEnvironmentVariables()
    {
        // arrange
        A.CallTo(() => _environment.GetEnvironmentVariables())
         .Returns(new Dictionary<string, string>
             {
                 { "x1", "y" },
                 { "x2", "yy" },
             });

        _context = CreateContext();
        IScope sut = CreateSut();

        sut.PushEnvironmentVariable(
            new Dictionary<string, string>
                {
                    { "x2", "yy2" },
                    { "x3", "yyy" },
                });
        sut.PushEnvironmentVariable(
            new Dictionary<string, string>
                {
                    { "x21", "yy2" },
                    { "x31", "yyy" },
                });

        // assume
        _context.Env.Count.Should().Be(5);

        // act
        sut.Dispose();

        // assert
        _context.Env.Count.Should().Be(2);
    }

    private ActionMenuGenerationContext CreateContext()
    {
        var context = new ActionMenuGenerationContext(
            _templateParser,
            new Core.Abstractions.OperatingSystem(_fileSystem, _environment),
            _functionsArray,
            _mapper,
            _deserializer,
            _mappers);
        context.Initialize(_repository);
        return context;
    }

    private DisposableContextScriptObject CreateSut()
    {
        return new DisposableContextScriptObject(_context, _mappers);
    }
}

file class EnabledContextAction : IContextAction, IEnabled
{
    public EnabledContextAction(string type, bool enabled)
    {
        Type = type;
        Enabled = enabled;
    }

    public string Type { get; }

    public Predicate Enabled { get; }
}