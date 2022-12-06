namespace RepoM.Plugin.Statistics.Tests;

using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using RepoM.Core.Plugin.Common;
using RepoM.Plugin.Statistics.Interface;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class RepositoryStatisticsTest
{
    [Fact]
    public void Recordings_ShouldBeEmpty_WhenConstructed()
    {
        // arrange
        IClock clock = A.Fake<IClock>();

        // act
        var sut = new RepositoryStatistics("path", clock);

        // assert
        sut.Recordings.Should().BeEmpty();
    }

    [Fact]
    public void Apply_ShouldAddTimestampToRecordings_WhenEventIsRepositoryActionRecordedEvent()
    {
        // arrange
        DateTime now = DateTime.Now.AddMinutes(-1000);
        IClock clock = A.Fake<IClock>();
        var sut = new RepositoryStatistics("path", clock);

        // act
        sut.Apply(new RepositoryActionRecordedEvent
            {
                Repository = "path",
                Timestamp = now,
            });

        // assert
        sut.Recordings.Should().BeEquivalentTo(new[] { now, });
    }

    [Fact]
    public void Apply_ShouldAddTimestampToRecordings_WhenEventIsRepositoryActionRecordedEventWithAlreadyOneDateInside()
    {
        // arrange
        DateTime now1 = DateTime.Now.AddMinutes(-1000);
        DateTime now2 = DateTime.Now.AddMinutes(-100);
        IClock clock = A.Fake<IClock>();
        var sut = new RepositoryStatistics("path", clock);
        sut.Apply(new RepositoryActionRecordedEvent
            {
                Repository = "path",
                Timestamp = now1,
            });

        // act
        sut.Apply(new RepositoryActionRecordedEvent
            {
                Repository = "path",
                Timestamp = now2,
            });

        // assert
        sut.Recordings.Should().BeEquivalentTo(new[] { now1, now2, });
    }

    [Fact]
    public void Apply_ShouldThrow_WhenEventTypeIsWrong()
    {
        // arrange
        IClock clock = A.Fake<IClock>();
        var sut = new RepositoryStatistics("path", clock);

        // act
        Action act = () => sut.Apply(new DummyEvent());
        
        // assert
        act.Should().Throw<NotImplementedException>();
    }


    [Fact]
    public void Record_ShouldAddTimestampToRecording()
    {
        // arrange
        DateTime now = DateTime.Now.AddMinutes(-1000);
        IClock clock = A.Fake<IClock>();
        A.CallTo(() => clock.Now).Returns(now);
        var sut = new RepositoryStatistics("path", clock);

        // act
        _ = sut.Record();

        // assert
        sut.Recordings.Should().BeEquivalentTo(new[] { now, });
    }

    [Fact]
    public async Task Record_ShouldReturnEvent()
    {
        // arrange
        var now = new DateTime(2022, 12, 13, 15, 17, 8, DateTimeKind.Local);
        IClock clock = A.Fake<IClock>();
        A.CallTo(() => clock.Now).Returns(now);
        var sut = new RepositoryStatistics("path", clock);

        // act
        IEvent evt = sut.Record();

        // assert
        evt.Should().BeOfType<RepositoryActionRecordedEvent>();
        await Verifier.Verify(evt).DontScrubDateTimes();
    }
}