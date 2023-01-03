namespace RepoM.Plugin.Heidi.Tests;

using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using RepoM.Plugin.Heidi.Internal;
using Xunit;

public class HeidiModuleTest
{
    [Fact]
    public void StartAsync_ShouldInitialize()
    {
        // arrange
        IHeidiConfigurationService heidiConfigurationService = A.Fake<IHeidiConfigurationService>();
        var task = Task.FromException(new Exception("throw by test, but doesn't matter"));
        A.CallTo(() => heidiConfigurationService.InitializeAsync()).Returns(task);
        var sut = new HeidiModule(heidiConfigurationService);

        // act
        Task result = sut.StartAsync();

        // assert
        A.CallTo(() => heidiConfigurationService.InitializeAsync()).MustHaveHappenedOnceExactly();
        result.Should().Be(task);
    }
}