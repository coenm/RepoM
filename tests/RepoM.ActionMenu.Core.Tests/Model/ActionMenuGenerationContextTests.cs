namespace RepoM.ActionMenu.Core.Tests.Model;

using FakeItEasy;
using FluentAssertions;
using System.IO.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;
using RepoM.ActionMenu.Core.Abstractions;
using RepoM.ActionMenu.Core.Misc;
using Xunit;
using RepoM.ActionMenu.Core.Model;

public class ActionMenuGenerationContextTests
{
    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange
        var os = new Core.Abstractions.OperatingSystem(A.Dummy<IFileSystem>(), A.Dummy<IEnvironment>());
        var tp = A.Dummy<ITemplateParser>();
        var amd = A.Dummy<IActionMenuDeserializer>();

        // act
        Func<ActionMenuGenerationContext> act1 = () => new ActionMenuGenerationContext(tp, os, [], [], amd, null!);
        Func<ActionMenuGenerationContext> act2 = () => new ActionMenuGenerationContext(tp, os, [], [], null!, []);
        Func<ActionMenuGenerationContext> act3 = () => new ActionMenuGenerationContext(tp, os, [], null!, amd, []);
        Func<ActionMenuGenerationContext> act4 = () => new ActionMenuGenerationContext(tp, os, null!, [], amd, []);
        Func<ActionMenuGenerationContext> act5 = () => new ActionMenuGenerationContext(tp, null!, [], [], amd, []);
        Func<ActionMenuGenerationContext> act6 = () => new ActionMenuGenerationContext(null!, os, [], [], amd, []);

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
        act3.Should().Throw<ArgumentNullException>();
        act4.Should().Throw<ArgumentNullException>();
        act5.Should().Throw<ArgumentNullException>();
        act6.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task AddActionMenusAsync_ShouldDoNothing_WhenArgumentIsNull()
    {
        // arrange
        var sut = new ActionMenuGenerationContext(
            A.Dummy<ITemplateParser>(),
            A.Dummy<Core.Abstractions.OperatingSystem>(),
            [],
            [],
            A.Dummy<IActionMenuDeserializer>(),
            []);

        // act
        var result = await sut.AddActionMenusAsync(null!).ToListAsync();

        // assert
        result.Should().BeEmpty();
    }
}