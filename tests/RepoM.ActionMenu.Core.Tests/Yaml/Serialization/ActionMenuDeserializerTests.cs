namespace RepoM.ActionMenu.Core.Tests.Yaml.Serialization;

using System.Collections.Generic;
using RepoM.ActionMenu.Core.Misc;
using RepoM.ActionMenu.Core.Yaml.Serialization;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using Xunit;

public class ActionMenuDeserializerTests
{
    private readonly FixedTemplateParser _templateParser = new FixedTemplateParser();
    private IEnumerable<IKeyTypeRegistration<IMenuAction>> _registrations = new List<IKeyTypeRegistration<IMenuAction>>();

    [Fact]
    public void Abc()
    {
        // arrange
        var sut = new ActionMenuDeserializer(_registrations, _templateParser);

        var result = sut.DeserializeRoot("");



        //
        //
        // var converter = new CustomTypeConverter();
        //
        // // Test converting from YAML to custom type
        // var yaml = @"name: John
        //          age: 30";
        // var expectedObject = new CustomObject { Name = "John", Age = 30 };
        // var actualObject = converter.ConvertFromYaml(yaml, typeof(CustomObject));
    }
}