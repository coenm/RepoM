namespace RepoM.ActionMenu.Core.Tests.ActionMenu.Context;

using System;
using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
using RepoM.ActionMenu.Core.ActionMenu.Context;
using Scriban.Parsing;
using Scriban;
using Scriban.Runtime;
using Xunit;

public class EnvScriptObjectTests
{
    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<EnvScriptObject> act1 = () => new EnvScriptObject(null!);

        // assert
        act1.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Count_ShouldReturn0_WhenDictionaryIsEmpty()
    {
        // arrange
        var sut = new EnvScriptObject(new Dictionary<string, string>());

        // act
        var result = sut.Count;

        // assert
        result.Should().Be(0);
    }

    [Fact]
    public void Count_ShouldReturnDictionaryCount()
    {
        // arrange
        var sut = new EnvScriptObject(new Dictionary<string, string>
            {
                { "dummy1", "dummy1" },
                { "dummy2", "dummy2" },
            });

        // act
        var result = sut.Count;

        // assert
        result.Should().Be(2);
    }

    [Fact]
    public void GetMembers_ShouldReturnMembers()
    {
        // arrange
        var sut = new EnvScriptObject(new Dictionary<string, string>
            {
                { "member_dummy1", "dummy1" },
                { "member_dummy2", "dummy2" },
            });

        // act
        IEnumerable<string> result = sut.GetMembers();

        // assert
        result.Should().BeEquivalentTo("member_dummy1", "member_dummy2");
    }

    [Fact]
    public void Contains_ShouldReturnMemberExistence()
    {
        // arrange
        var sut = new EnvScriptObject(new Dictionary<string, string>
            {
                { "member_dummy1", "dummy1" },
                { "member_dummy2", "dummy2" },
            });

        // act
        var result1 = sut.Contains("member1");
        var result2 = sut.Contains("member_dummy1");

        // assert
        result1.Should().BeFalse();
        result2.Should().BeTrue();
    }

    [Fact]
    public void TryGetValue_ShouldReturnValue_WhenExists()
    {
        // arrange
        var sut = new EnvScriptObject(new Dictionary<string, string>
            {
                { "member_dummy1", "dummy1" },
                { "member_dummy2", "dummy2" },
            });

        // act
        var result1 = sut.TryGetValue(A.Dummy<TemplateContext>(), A.Dummy<SourceSpan>(), "member1", out var value1);
        var result2 = sut.TryGetValue(A.Dummy<TemplateContext>(), A.Dummy<SourceSpan>(), "member_dummy1", out var value2);

        // assert
        result1.Should().BeFalse();
        value1.Should().BeNull();
        result2.Should().BeTrue();
        value2.Should().Be("dummy1");
    }

    [Fact]
    public void CanWrite_ShouldReturnFalse()
    {
        // arrange
        var sut = new EnvScriptObject(new Dictionary<string, string>
            {
                { "member_dummy1", "dummy1" },
                { "member_dummy2", "dummy2" },
            });

        // act
        var result1 = sut.CanWrite("member1");
        var result2 = sut.CanWrite("member_dummy1");

        // assert
        result1.Should().BeFalse();
        result2.Should().BeFalse();
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TrySetValue_ShouldReturnFalseAndNotUpdateValues(bool readOnly)
    {
        // arrange
        var sut = new EnvScriptObject(new Dictionary<string, string>
            {
                { "member_dummy1", "dummy1" },
                { "member_dummy2", "dummy2" },
            });

        // act
        var result1 = sut.TrySetValue(A.Dummy<TemplateContext>(), A.Dummy<SourceSpan>(), "member1", "value", readOnly);
        var result2 = sut.TrySetValue(A.Dummy<TemplateContext>(), A.Dummy<SourceSpan>(), "member_dummy1", "value2", readOnly);

        // assert
        result1.Should().BeFalse();
        result2.Should().BeFalse();
    }


    [Fact]
    public void Remove_ShouldNeverRemove()
    {
        // arrange
        var sut = new EnvScriptObject(new Dictionary<string, string>
            {
                { "member_dummy1", "dummy1" },
                { "member_dummy2", "dummy2" },
            });

        // act
        var result1 = sut.Remove("member1");
        var result2 = sut.Remove("member_dummy1");

        // assert
        result1.Should().BeFalse();
        result2.Should().BeFalse();
        sut.GetMembers().Should().BeEquivalentTo("member_dummy1", "member_dummy2");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Clone_ShouldReturnCloned(bool deepClone)
    {
        // arrange
        var sut = new EnvScriptObject(new Dictionary<string, string>
            {
                { "member_dummy1", "dummy1" },
                { "member_dummy2", "dummy2" },
            });

        // act
        IScriptObject clone = ((IScriptObject)sut).Clone(deepClone);

        // assert
        sut.GetMembers().Should().BeEquivalentTo("member_dummy1", "member_dummy2");
        clone.GetMembers().Should().BeEquivalentTo("member_dummy1", "member_dummy2");
        clone.Should().NotBeSameAs(sut);
    }
}