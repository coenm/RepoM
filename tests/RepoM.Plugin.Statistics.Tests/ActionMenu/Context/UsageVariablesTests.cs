namespace RepoM.Plugin.Statistics.Tests.ActionMenu.Context;

using System;
using FakeItEasy;
using System.Collections.Generic;
using FluentAssertions;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.Statistics;
using RepoM.Plugin.Statistics.ActionMenu.Context;
using Xunit;

public class UsageVariablesTests
{
    private readonly IActionMenuGenerationContext _context = A.Fake<IActionMenuGenerationContext>();
    private readonly IStatisticsService _service = A.Fake<IStatisticsService>();
    private readonly IRepository _repository = A.Fake<IRepository>();
    private readonly UsageVariables _sut;

    public UsageVariablesTests()
    {
        A.CallTo(() => _context.Repository).Returns(_repository);
        _sut = new UsageVariables(_service);
    }

    [Fact]
    public void GetCount_ShouldCallAndReturnService_WhenCalled()
    {
        // arrange
        A.CallTo(() => _service.GetRecordings(_repository)).Returns(new List<DateTime> { DateTime.Now, DateTime.Now, });

        // act
        var result = _sut.GetCount(_context);

        // assert
        result.Should().Be(2);
        A.CallTo(() => _service.GetRecordings(_repository)).MustHaveHappenedOnceExactly();
        A.CallTo(_service).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void GetOverallCount_ShouldCallAndReturnService_WhenCalled()
    {
        // arrange
        A.CallTo(() => _service.GetTotalRecordingCount()).Returns(42);

        // act
        var result = _sut.GetOverallCount();

        // assert
        result.Should().Be(42);
        A.CallTo(() => _service.GetTotalRecordingCount()).MustHaveHappenedOnceExactly();
        A.CallTo(_service).MustHaveHappenedOnceExactly();
    }
}