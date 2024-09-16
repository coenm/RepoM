namespace RepoM.App.Tests.Services;

using FluentAssertions;
using RepoM.App.Services;
using Xunit;

public class TaskBarTests
{
    [Fact]
    public void GetTaskBarLocation_ShouldReturnBottom_WhenNoScreenGiven()
    {
        // arrange

        // act
        TaskBar.TaskBarLocation result = TaskBar.GetTaskBarLocation(null);

        // assert
        result.Should().Be(TaskBar.TaskBarLocation.Bottom);
    }
}