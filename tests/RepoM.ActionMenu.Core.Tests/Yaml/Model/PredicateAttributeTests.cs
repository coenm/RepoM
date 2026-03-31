namespace RepoM.ActionMenu.Core.Tests.Yaml.Model;

using System;
using AwesomeAssertions;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using Xunit;

public class PredicateAttributeTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Ctor_ShouldSetDefaultValue(bool defaultValue)
    {
        // act
        var sut = new PredicateAttribute(defaultValue);

        // assert
        sut.DefaultValue.Should().Be(defaultValue);
    }

    [Fact]
    public void PredicateAttribute_ShouldBeAnAttribute()
    {
        // act
        var sut = new PredicateAttribute(true);

        // assert
        sut.Should().BeAssignableTo<Attribute>();
    }

    [Fact]
    public void PredicateAttribute_ShouldDeriveFromEvaluateToAttribute()
    {
        // act
        var sut = new PredicateAttribute(false);

        // assert
        sut.Should().BeAssignableTo<EvaluateToAttribute>();
    }
}
