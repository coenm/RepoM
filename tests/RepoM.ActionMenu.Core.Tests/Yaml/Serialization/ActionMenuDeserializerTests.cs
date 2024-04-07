namespace RepoM.ActionMenu.Core.Tests.Yaml.Serialization;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
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
        _sut = new ActionMenuDeserializer(_registrations, _templateParser);
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