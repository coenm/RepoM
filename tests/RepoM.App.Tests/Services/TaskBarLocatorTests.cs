namespace RepoM.App.Tests.Services;

using FluentAssertions;
using RepoM.App.Services;
using Xunit;

public class TaskBarLocatorTests
{
    [Fact]
    public void GetTaskBarLocation_ShouldReturnBottom_WhenNoScreenGiven()
    {
        // arrange

        // act
        TaskBarLocator.TaskBarLocation result = TaskBarLocator.GetTaskBarLocation(null);

        // assert
        result.Should().Be(TaskBarLocator.TaskBarLocation.Bottom);
    }
}