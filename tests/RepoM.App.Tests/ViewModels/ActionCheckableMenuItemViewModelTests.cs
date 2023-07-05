namespace RepoM.App.Tests.ViewModels;

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using RepoM.App.ViewModels;
using Xunit;

[SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer", Justification = "Test AAA readability.")]
public class ActionCheckableMenuItemViewModelTests
{
    private bool _isSelectedValue;
    private bool _isSelectedFuncCalled;
    private readonly Func<bool> _isSelectedFunc;
    private readonly Action _setKeyFunc;
    private bool _setKeyFuncCalled;

    public ActionCheckableMenuItemViewModelTests()
    {
        _isSelectedFuncCalled = false;
        _isSelectedFunc = () =>
            {
                _isSelectedFuncCalled = true;
                return _isSelectedValue;
            };
        _setKeyFuncCalled = false;
        _setKeyFunc = () => _setKeyFuncCalled = true;
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentsNull()
    {
        // arrange

        // act
        var act1 = () => _ = new ActionCheckableMenuItemViewModel(_isSelectedFunc, _setKeyFunc, null!);
        var act2 = () => _ = new ActionCheckableMenuItemViewModel(_isSelectedFunc, _setKeyFunc, string.Empty);
        var act3 = () => _ = new ActionCheckableMenuItemViewModel(_isSelectedFunc, _setKeyFunc, " ");
        var act4 = () => _ = new ActionCheckableMenuItemViewModel(_isSelectedFunc, null!, "dummy");
        var act5 = () => _ = new ActionCheckableMenuItemViewModel(null!, _setKeyFunc, "dummy");

        // assert
        act1.Should().ThrowExactly<ArgumentNullException>();
        act2.Should().ThrowExactly<ArgumentNullException>();
        act3.Should().ThrowExactly<ArgumentNullException>();
        act4.Should().ThrowExactly<ArgumentNullException>();
        act5.Should().ThrowExactly<ArgumentNullException>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsChecked_ShouldCallAndReturnGetAction_WhenGet(bool isSelectedValue)
    {
        // arrange
        _isSelectedValue = isSelectedValue;
        var sut = new ActionCheckableMenuItemViewModel(_isSelectedFunc, _setKeyFunc, "dummy title");

        // act
        var result = sut.IsChecked;

        // assert
        result.Should().Be(isSelectedValue);
        _isSelectedFuncCalled.Should().BeTrue();
        _setKeyFuncCalled.Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsChecked_ShouldCallSetAction_WhenSet(bool value)
    {
        // arrange
        var sut = new ActionCheckableMenuItemViewModel(_isSelectedFunc, _setKeyFunc, "dummy title");

        // act
        sut.IsChecked = value;

        // assert
        _isSelectedFuncCalled.Should().BeFalse();
        _setKeyFuncCalled.Should().BeTrue();
    }
}