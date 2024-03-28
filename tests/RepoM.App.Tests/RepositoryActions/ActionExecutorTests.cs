namespace RepoM.App.Tests.RepositoryActions;

using System;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RepoM.App.RepositoryActions;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using SimpleInjector;
using Xunit;

public class ActionExecutorTests
{
    private readonly Container _container = new();
    private readonly IRepository _repository = A.Dummy<IRepository>();
    private readonly ActionExecutor _sut;

    public ActionExecutorTests()
    {
        _sut = new ActionExecutor(_container, A.Dummy<ILogger>());
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentIsNull()
    {
        // arrange

        // act
        Func<ActionExecutor> act1 = () => _ = new ActionExecutor(_container, null!);
        Func<ActionExecutor> act2 = () => _ = new ActionExecutor(null!, A.Dummy<ILogger>());

        // assert
        act1.Should().ThrowExactly<ArgumentNullException>();
        act2.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Execute_ShouldReturnFalse_WhenCommandExecutorNotRegistered()
    {
        // arrange
        IRepositoryCommand command = new DummyRepositoryCommand();

        // act
        var result = _sut.Execute(_repository, command);

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Execute_ShouldReturnTrue_WhenCommandExecutorWasFoundAndExecutedSuccessfully()
    {
        // arrange
        _container.Register<ICommandExecutor<DummyRepositoryCommand>, DummyRepositoryCommandExecutor>(Lifestyle.Singleton);
        IRepositoryCommand command = new DummyRepositoryCommand();

        // act
        var result = _sut.Execute(_repository, command);

        // assert
        result.Should().BeTrue();
    }

    public class DummyRepositoryCommand : IRepositoryCommand
    {
        // intentionally empty
    }

    public class DummyRepositoryCommandExecutor : ICommandExecutor<DummyRepositoryCommand>
    {
        public void Execute(IRepository repository, DummyRepositoryCommand action)
        {
            // intentionally empty
        }
    }
}