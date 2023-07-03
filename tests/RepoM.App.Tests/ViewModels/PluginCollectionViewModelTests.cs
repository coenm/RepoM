namespace RepoM.App.Tests.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using RepoM.App.Plugins;
using RepoM.App.ViewModels;
using Xunit;

public class PluginCollectionViewModelTests
{
    private readonly IModuleManager _moduleManager;

    public PluginCollectionViewModelTests()
    {
        _moduleManager = A.Fake<IModuleManager>();
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentsNull()
    {
        // arrange

        // act
        var act = () => _ = new PluginCollectionViewModel(null!);

        // assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void Ctor_ShouldInitializePlugins(bool enabled, bool found)
    {
        // arrange
        A.CallTo(() => _moduleManager.Plugins)
         .Returns(new List<PluginModel>
            {
                new(enabled, (_, _) => { })
                    {
                        Dll = "Dllx",
                        Name = "Namex",
                        Found = found,
                    },
            });
         
        var sut = new PluginCollectionViewModel(_moduleManager);

        // act
        var result = sut.Single();
        
        // assert
        result.Header.Should().Be("Namex");
        result.IsCheckable.Should().Be(found);
    }
}