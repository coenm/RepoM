namespace RepoM.ActionMenu.Core.Tests.Yaml.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.ActionMenu.Core.Misc;
using RepoM.ActionMenu.Core.Yaml.Model;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext.SetVariable;
using RepoM.ActionMenu.Core.Yaml.Serialization;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using VerifyXunit;
using Xunit;

public class ActionMenuDeserializerTests
{
    private readonly FixedTemplateParser _templateParser = new();
    private readonly IEnumerable<IKeyTypeRegistration<IMenuAction>> _registrations = [];
    private readonly ActionMenuDeserializer _sut;

    public ActionMenuDeserializerTests()
    {
        _sut = new ActionMenuDeserializer(_registrations, _templateParser, NullLogger.Instance);
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange
        IEnumerable<IKeyTypeRegistration<IMenuAction>> keyTypeRegistrations = A.Fake<IEnumerable<IKeyTypeRegistration<IMenuAction>>>();
        ITemplateParser templateParser = A.Dummy<ITemplateParser>();
        ILogger logger = A.Dummy<ILogger>();
        
        // act
        Func<ActionMenuDeserializer> act1 = () => new ActionMenuDeserializer(keyTypeRegistrations, templateParser, null!);
        Func<ActionMenuDeserializer> act2 = () => new ActionMenuDeserializer(keyTypeRegistrations, null!, logger);
        Func<ActionMenuDeserializer> act3 = () => new ActionMenuDeserializer(null!, templateParser, logger);

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
        act3.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task DeserializeContextRoot_ShouldHandleYaml_WhenContextCanBeDeserializedWithDefaultContextActionTypeConverter()
    {
        // arrange
        const string YAML = """
                            context:
                            - variable_name: 123
                            """;

        // act
        ContextRoot result = _sut.Deserialize<ContextRoot>(YAML);

        // assert
        ContextActionSetVariableV1 contextActionSetVariable = result.Context!.Single().Should().BeOfType<ContextActionSetVariableV1>().Subject;
        contextActionSetVariable.Value.Should().BeOfType<string>();
        await Verifier.Verify(contextActionSetVariable);
    }
}