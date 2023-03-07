namespace RepoM.Api.Tests.Common;

using System;
using FakeItEasy;
using FluentAssertions;
using RepoM.Api.Common;
using RepoM.Core.Plugin.Common;
using Xunit;

public class HardcodedMiniHumanizerTests
{
    private readonly IClock _clock;
    private readonly HardcodededMiniHumanizer _sut;

    public HardcodedMiniHumanizerTests()
    {
        _clock = A.Fake<IClock>();
        A.CallTo(() => _clock.Now).Returns(new DateTime(2020, 01, 01, 10, 00, 00));
        _sut = new HardcodededMiniHumanizer(_clock);
    }

    [Fact]
    public void Returns_Speaking_Now()
    {
        var value = _sut.HumanizeTimestamp(_clock.Now);
        value.Should().Be("Just now");
    }

    [Theory]
    [InlineData(0, "Just now")]
    [InlineData(-20, "Just now")]
    [InlineData(20, "Just now")]
    [InlineData(-30, "30 seconds ago")]
    [InlineData(30, "In 30 seconds")]
    [InlineData(-30.4123123, "30 seconds ago")]
    [InlineData(30.4123123, "In 30 seconds")]
    [InlineData(-45, "45 seconds ago")]
    [InlineData(45, "In 45 seconds")]
    [InlineData(-55, "A minute ago")]
    [InlineData(55, "In a minute")]
    [InlineData(-60, "A minute ago")]
    [InlineData(60, "In a minute")]
    [InlineData(-89, "Nearly two minutes ago")]
    [InlineData(89, "In nearly two minutes")]
    [InlineData(-90, "Nearly two minutes ago")]
    [InlineData(90, "In nearly two minutes")]
    [InlineData(-101, "2 minutes ago")]
    [InlineData(101, "In 2 minutes")]
    [InlineData(-300, "5 minutes ago")]
    [InlineData(300, "In 5 minutes")]
    public void Seconds(double seconds, string expected)
    {
        var value = _sut.HumanizeTimestamp(_clock.Now.AddSeconds(seconds));
        value.Should().Be(expected);
    }

    [Theory]
    [InlineData(-15, "15 minutes ago")]
    [InlineData(15, "In 15 minutes")]
    [InlineData(-30.4123123, "30 minutes ago")]
    [InlineData(30.4123123, "In 30 minutes")]
    [InlineData(-45, "45 minutes ago")]
    [InlineData(45, "In 45 minutes")]
    [InlineData(-57, "An hour ago")]
    [InlineData(57, "In an hour")]
    [InlineData(-60, "An hour ago")]
    [InlineData(60, "In an hour")]
    [InlineData(-71, "An hour ago")]
    [InlineData(71, "In an hour")]
    [InlineData(-89, "One and a half hour ago")]
    [InlineData(89, "In one and a half hour")]
    [InlineData(-90, "One and a half hour ago")]
    [InlineData(90, "In one and a half hour")]
    [InlineData(-110, "2 hours ago")]
    [InlineData(110, "In 2 hours")]
    [InlineData(-120, "2 hours ago")]
    [InlineData(120, "In 2 hours")]
    [InlineData(-130, "2 hours ago")]
    [InlineData(130, "In 2 hours")]
    [InlineData(-300, "5 hours ago")]
    [InlineData(300, "In 5 hours")]
    public void Minutes(double minutes, string expected)
    {
        var value = _sut.HumanizeTimestamp(_clock.Now.AddMinutes(minutes));
        value.Should().Be(expected);
    }

    [Theory]
    [InlineData(-0, "Just now")]
    [InlineData(-12, "12 hours ago")]
    [InlineData(12, "In 12 hours")]
    [InlineData(-12.4123123, "12 hours ago")]
    [InlineData(12.4123123, "In 12 hours")]
    [InlineData(-22, "22 hours ago")]
    [InlineData(22, "In 22 hours")]
    [InlineData(-23, "A day ago")]
    [InlineData(23, "In a day")]
    [InlineData(-24, "A day ago")]
    [InlineData(24, "In a day")]
    [InlineData(-30, "A day ago")]
    [InlineData(30, "In a day")]
    [InlineData(-40, "2 days ago")]
    [InlineData(40, "In 2 days")]
    [InlineData(-50, "2 days ago")]
    [InlineData(50, "In 2 days")]
    [InlineData(-60, "2 days ago")]
    [InlineData(60, "In 2 days")]
    [InlineData(-62, "3 days ago")]
    [InlineData(62, "In 3 days")]
    public void Hours(double hours, string expected)
    {
        var value = _sut.HumanizeTimestamp(_clock.Now.AddHours(hours));
        value.Should().Be(expected);
    }

    [Fact]
    public void Date_Fallback()
    {
        var value = _sut.HumanizeTimestamp(_clock.Now.AddHours(-300));
        value.Should().Be(new DateTime(2019, 12, 19, 22, 00, 00).ToString("g"));

        value = _sut.HumanizeTimestamp(_clock.Now.AddHours(300));
        value.Should().Be(new DateTime(2020, 01, 13, 22, 00, 00).ToString("g"));
    }
}