namespace RepoM.Plugin.Statistics.Tests.RepositoryActions;

using System;
using FakeItEasy;
using FluentAssertions;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Commands;
using RepoM.Plugin.Statistics.RepositoryActions;
using Xunit;

public class RecordStatisticsActionExecutorDecoratorTest
{
    private readonly IClock _clock;
    private readonly IRepository _repository;
    private readonly DateTime _now = DateTime.Now;

    public RecordStatisticsActionExecutorDecoratorTest()
    {
        _clock = A.Dummy<IClock>();
        A.CallTo(() => _clock.Now).Returns(_now);
        _repository = A.Fake<IRepository>();
        A.CallTo(() => _repository.SafePath).Returns("C:/path/repo");
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        var act1 = () => new RecordStatisticsActionExecutorDecorator<DummyRepositoryCommand>(A.Dummy<IActionExecutor<DummyRepositoryCommand>>(), null!);
        var act2 = () => new RecordStatisticsActionExecutorDecorator<DummyRepositoryCommand>(null!, A.Dummy<IStatisticsService>());

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
    }


    [Fact]
    public void Execute_ShouldCallExecuteOnDecorateeWithSameArguments()
    {
        // arrange
        IActionExecutor<DummyRepositoryCommand> decoratee = A.Fake<IActionExecutor<DummyRepositoryCommand>>();
        var service = new StatisticsService(_clock);
        var sut = new RecordStatisticsActionExecutorDecorator<DummyRepositoryCommand>(decoratee, service);

        // act
        var action = new DummyRepositoryCommand();
        sut.Execute(_repository, action);

        // assert
        _ = A.CallTo(() => decoratee.Execute(_repository, action)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Execute_ShouldCallExecuteOnDecorateeWithSameArguments_WhenRecordFailsOnService()
    {
        // arrange
        IActionExecutor<DummyRepositoryCommand> decoratee = A.Fake<IActionExecutor<DummyRepositoryCommand>>();
        IStatisticsService service = A.Fake<IStatisticsService>();
        A.CallTo(() => service.Record(A<IRepository>._)).Throws(new Exception("Thrown by test"));
        var sut = new RecordStatisticsActionExecutorDecorator<DummyRepositoryCommand>(decoratee, service);

        // act
        var action = new DummyRepositoryCommand();
        sut.Execute(_repository, action);

        // assert
        A.CallTo(() => service.Record(_repository)).MustHaveHappenedOnceExactly();
        _ = A.CallTo(() => decoratee.Execute(_repository, action)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Execute_ShouldRecordWhenTypeIsNotNullType()
    {
        // arrange
        IActionExecutor<DummyRepositoryCommand> decoratee = A.Fake<IActionExecutor<DummyRepositoryCommand>>();
        var service = new StatisticsService(_clock);
        var sut = new RecordStatisticsActionExecutorDecorator<DummyRepositoryCommand>(decoratee, service);

        // act
        var action = new DummyRepositoryCommand();
        sut.Execute(_repository, action);

        // assert
        service.GetRecordings(_repository).Should().BeEquivalentTo(new[] { _now, });
    }
    
    [Fact]
    public void Execute_ShouldNotRecordWhenTypeIsNullType()
    {
        // arrange
        IActionExecutor<NullRepositoryCommand> decoratee = A.Fake<IActionExecutor<NullRepositoryCommand>>();
        var service = new StatisticsService(_clock);
        var sut = new RecordStatisticsActionExecutorDecorator<NullRepositoryCommand>(decoratee, service);

        // act
        sut.Execute(_repository, NullRepositoryCommand.Instance);

        // assert
        service.GetRecordings(_repository).Should().BeEmpty();
    }
}

public class DummyRepositoryCommand : IRepositoryCommand
{
}