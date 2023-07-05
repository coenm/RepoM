namespace RepoM.App.Tests.ViewModels;

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using RepoM.App.Plugins;
using RepoM.App.ViewModels;
using Xunit;

public class PluginViewModelTests
{
    private readonly PluginModel _pluginModelFound;
    private readonly PluginModel _pluginModelNotFound;

    public PluginViewModelTests()
    {
        _pluginModelFound = new PluginModel(true, (_, _) => { })
            {
                Name = "DummyNameTrue",
                Found = true,
            };
        _pluginModelNotFound = new PluginModel(true, (_, _) => { })
            {
                Name = "DummyNameFalse",
                Found = false,
            };
    }

    [Fact]
    public void Ctor_ShouldThrown_WhenArgumentIsNull()
    {
        // arrange

        // act
        Action act1 = () => _ = new PluginViewModel(null!);

        // assert
        act1.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Header_ShouldBeNameOfModel()
    {
        // arrange
        var sut = new PluginViewModel(_pluginModelFound);

        // act
        var result = sut.Header;

        // assert
        result.Should().Be("DummyNameTrue");
    }

    [Fact]
    public void IsCheckable_ShouldBeFalse_WhenModelIsNotFound()
    {
        // arrange
        var sut = new PluginViewModel(_pluginModelNotFound);

        // act
        var result = sut.IsCheckable;

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsCheckable_ShouldBeTrue_WhenModelIsFound()
    {
        // arrange
        var sut = new PluginViewModel(_pluginModelFound);

        // act
        var result = sut.IsCheckable;

        // assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsChecked_ShouldReflectModelEnabled(bool enabled)
    {
        // arrange
        var model = new PluginModel(enabled, (_, _) => { });
        var sut = new PluginViewModel(model);

        // act
        var result = sut.IsChecked;

        // assert
        result.Should().Be(enabled);
    }

    [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer", Justification = "Readability")]
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsChecked_ShouldUseCheckValueAndDllName_WhenSet(bool isCheckedValue)
    {
        // arrange
        bool? capturedBool = null;
        string? capturedString = null;
        var model = new PluginModel(true, (s, b) =>
            {
                capturedBool = b;
                capturedString = s;
            })
            {
                Dll = "myDllName",
            };
        var sut = new PluginViewModel(model);

        // act
        sut.IsChecked = isCheckedValue;

        // assert
        capturedBool.Should().Be(isCheckedValue);
        capturedString.Should().Be("myDllName");
    }
}