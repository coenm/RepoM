namespace RepoM.Plugin.Clipboard.Tests.RepositoryAction;

using System;
using FakeItEasy;
using FluentAssertions;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.Clipboard.RepositoryAction;
using RepoM.Plugin.Clipboard.RepositoryAction.Actions;
using TextCopy;
using Xunit;

public class CopyToClipboardCommandExecutorTests
{
    private readonly IClipboard _clipboard;
    private readonly CopyToClipboardCommandExecutor _sut;
    private readonly IRepository _repository;

    public CopyToClipboardCommandExecutorTests()
    {
        _clipboard = A.Fake<IClipboard>();
        _sut = new CopyToClipboardCommandExecutor(_clipboard);
        _repository = A.Fake<IRepository>();
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentIsNull()
    {
        // arrange

        // act
        Action act = () => _= new CopyToClipboardCommandExecutor(null!);

        // assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Theory]
    [InlineData("text213")]
    [InlineData("text 213")]
    [InlineData("")]
    public void Execute_ShouldSetTextToClipboard(string text)
    {
        // arrange
        
        // act
        _sut.Execute(_repository, new CopyToClipboardRepositoryCommand(text));

        // assert
        A.CallTo(() => _clipboard.SetText(text)).MustHaveHappenedOnceExactly();
        A.CallTo(_clipboard).MustHaveHappenedOnceExactly();
    }
}