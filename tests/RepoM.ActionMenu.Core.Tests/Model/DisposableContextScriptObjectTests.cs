namespace RepoM.ActionMenu.Core.Tests.Model;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using FakeItEasy;
using RepoM.ActionMenu.Core.ActionMenu.Context;
using RepoM.ActionMenu.Core.ConfigReader;
using RepoM.ActionMenu.Core.Misc;
using RepoM.ActionMenu.Core.Model;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext;
using RepoM.ActionMenu.Core.Yaml.Serialization;
using RepoM.ActionMenu.Interface.Scriban;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Core.Plugin.Repository;
using Xunit;

public class DisposableContextScriptObjectTests
{
    private readonly IRepository _repository = A.Fake<IRepository>();
    private readonly ITemplateParser _templateParser = new FixedTemplateParser();
    private readonly IFileSystem _fileSystem = new MockFileSystem();
    private readonly ITemplateContextRegistration[] _functionsArray = Array.Empty<ITemplateContextRegistration>();
    private readonly IActionToRepositoryActionMapper[] _mapper = Array.Empty<IActionToRepositoryActionMapper>();
    private readonly IActionMenuDeserializer _deserializer = A.Fake<IActionMenuDeserializer>();
    private readonly IFileReader _fileReader = A.Fake<IFileReader>();
    private readonly ActionMenuGenerationContext _context;
    private readonly EnvSetScriptObject _env = new(new EnvScriptObject(new Dictionary<string, string>()));
    private readonly List<IContextActionProcessor> _mappers;
    private DisposableContextScriptObject _sut;

    public DisposableContextScriptObjectTests()
    {
        _context = new ActionMenuGenerationContext(_repository, _templateParser, _fileSystem, _functionsArray, _mapper, _deserializer, _fileReader);
        _mappers = new List<IContextActionProcessor>
            {
                A.Fake<IContextActionProcessor>(),
                A.Fake<IContextActionProcessor>(),
            };
        _sut = new DisposableContextScriptObject(_context, _env, _mappers);
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