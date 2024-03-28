namespace RepoM.Plugin.WebBrowser.Tests.RepositoryActions;

using System;
using FakeItEasy;
using FluentAssertions;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Plugin.WebBrowser.RepositoryActions;
using Xunit;

using CoreCommand = Core.Plugin.RepositoryActions.Commands;
using PluginCommand = RepoM.Plugin.WebBrowser.RepositoryActions.Actions;

public class CoreBrowseRepositoryCommandExecutorDecoratorTests
{
    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<CoreBrowseRepositoryCommandExecutorDecorator> act1 = () => new CoreBrowseRepositoryCommandExecutorDecorator(null!, A.Fake<ICommandExecutor<PluginCommand.BrowseRepositoryCommand>>());
        Func<CoreBrowseRepositoryCommandExecutorDecorator> act2 = () => new CoreBrowseRepositoryCommandExecutorDecorator(A.Fake<ICommandExecutor<CoreCommand.BrowseRepositoryCommand>>(), null!);

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Execute_ShouldMapCommandAndCallExecutor()
    {
        // arrange
        IRepository repository = A.Fake<IRepository>();
        ICommandExecutor<CoreCommand.BrowseRepositoryCommand> decoratee = A.Fake<ICommandExecutor<CoreCommand.BrowseRepositoryCommand>>();
        ICommandExecutor<PluginCommand.BrowseRepositoryCommand> pluginBrowseCommandExecutor = A.Fake<ICommandExecutor<PluginCommand.BrowseRepositoryCommand>>();
        var sut = new CoreBrowseRepositoryCommandExecutorDecorator(decoratee, pluginBrowseCommandExecutor);

        // act
        sut.Execute(repository, new CoreCommand.BrowseRepositoryCommand("http://www.github.com"));

        // assert
        A.CallTo(() => pluginBrowseCommandExecutor.Execute(repository, A<PluginCommand.BrowseRepositoryCommand>._)).MustHaveHappenedOnceExactly();
        A.CallTo(decoratee).MustNotHaveHappened();
    }
}