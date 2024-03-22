namespace RepoM.ActionMenu.Core.Tests.Misc;

using System;
using FluentAssertions;
using RepoM.ActionMenu.Core.Misc;
using Xunit;

public class FastStackTests
{
    private FastStack<int> _sut = new(5);

    [Fact]
    public void Push_ShouldAddItem()
    {
        // arrange

        // act
        _sut.Push(42);

        // assert
        _sut.Items.Should().BeEquivalentTo(new[] { 42, default, default, default, default, });
    }

    [Fact]
    public void Push_ShouldAddItemTwice()
    {
        // arrange
        _sut.Push(42);

        // act
        _sut.Push(44);

        // assert
        _sut.Items.Should().BeEquivalentTo(new[] { 42, 44, default, default, default, });
    }

    [Fact]
    public void Push_ShouldEnlargeInternalArray_WhenFull()
    {
        // arrange
        _sut.Push(1);
        _sut.Push(2);
        _sut.Push(3);
        _sut.Push(4);
        _sut.Push(5);

        // act
        _sut.Push(42);

        // assert
        _sut.Items.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5, 42, default, default, default, default, });
    }

    [Fact]
    public void Peek_ShouldGetLastAddedItem()
    {
        // arrange
        _sut.Push(1);
        _sut.Push(2);

        // act
        var result = _sut.Peek();

        // assert
        result.Should().Be(2);
    }

    [Fact]
    public void Peek_ShouldAlwaysGetLastAddedItem()
    {
        // arrange
        _sut.Push(1);
        _sut.Push(2);

        // act
        _ = _sut.Peek();
        var result = _sut.Peek();

        // assert
        result.Should().Be(2);
    }

    [Fact]
    public void Peek_ShouldNotDecreaseCount()
    {
        // arrange
        _sut.Push(1);
        _sut.Push(2);

        // act
        _ = _sut.Peek();

        // assert
        _sut.Count.Should().Be(2);
    }

    [Fact]
    public void Peek_ShouldThrow_WhenEmpty()
    {
        // arrange

        // act
        Func<int> act = () => _sut.Peek();

        // assert
        act.Should().Throw<InvalidOperationException>();
    }
}