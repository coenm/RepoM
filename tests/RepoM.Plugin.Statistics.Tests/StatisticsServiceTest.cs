namespace RepoM.Plugin.Statistics.Tests;

using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.Statistics.Interface;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class StatisticsServiceTest
{
    private readonly StatisticsService _sut;
    private readonly IRepository _repository1;
    private readonly IRepository _repository2;

    public StatisticsServiceTest()
    {
        IClock clock = A.Fake<IClock>();
        _sut = new StatisticsService(clock);

        _repository1 = A.Fake<IRepository>();
        A.CallTo(() => _repository1.SafePath).Returns("C:\\repo1");

        _repository2 = A.Fake<IRepository>();
        A.CallTo(() => _repository2.SafePath).Returns("C:\\repo2");
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<StatisticsService> act1 = () => new StatisticsService(null!);

        // assert
        act1.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetTotalRecordingCount_ShouldReturnZero_WhenJustInitialized()
    {
        // arrange

        // act
        var result = _sut.GetTotalRecordingCount();

        // assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetTotalRecordingCount_ShouldReturnOne_WhenSingleRepoIsRecorded()
    {
        // arrange
        _sut.Record(_repository1);
        
        // act
        var result = _sut.GetTotalRecordingCount();

        // assert
        result.Should().Be(1);
    }

    [Fact]
    public void GetTotalRecordingCount_ShouldReturnTwo_WhenTwoRepoIsRecorded()
    {
        // arrange
        _sut.Record(_repository1);
        _sut.Record(_repository2);
        
        // act
        var result = _sut.GetTotalRecordingCount();

        // assert
        result.Should().Be(2);
    }

    [Fact]
    public void Apply_ShouldThrow_WhenEventTypeIsUnknown()
    {
        // arrange
        IEvent evt = A.Fake<IEvent>();
        A.CallTo(() => evt.Repository).Returns("C:\\repo1");
        A.CallTo(() => evt.Timestamp).Returns(new DateTime(2020, 6, 14, 20, 30, 40));

        // act
        Action act = () => _sut.Apply(evt);

        // assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Type 'ObjectProxy_*' is unknown");
    }

    [Fact]
    public async Task Apply_ShouldAddEventInRepositoryRecording()
    {
        // arrange
        IEvent evt = new RepositoryActionRecordedEvent
            {
                Repository = "C:\\repo1",
                Timestamp = new DateTime(2020, 6, 14, 20, 30, 40),
            };

        // act
        _sut.Apply(evt);

        // assert
        IReadOnlyRepositoryStatistics? recordings = _sut.GetRepositoryRecording(_repository1);
        await Verifier.Verify(recordings);
    }

    [Fact]
    public async Task Apply_ShouldAddSecondEvent_WhenRepositoryIsSame()
    {
        // arrange
        IEvent evt1 = new RepositoryActionRecordedEvent
            {
                Repository = "C:\\repo1",
                Timestamp = new DateTime(2020, 6, 14, 20, 30, 40),
            };
        IEvent evt2 = new RepositoryActionRecordedEvent
            {
                Repository = "C:\\repo1",
                Timestamp = new DateTime(2020, 7, 7, 7, 7, 7),
            };

        // act
        _sut.Apply(evt1);
        _sut.Apply(evt2);

        // assert
        IReadOnlyRepositoryStatistics? recordings = _sut.GetRepositoryRecording(_repository1);
        await Verifier.Verify(recordings);
    }
}