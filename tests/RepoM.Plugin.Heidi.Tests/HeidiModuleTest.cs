namespace RepoM.Plugin.Heidi.Tests;

using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using RepoM.Plugin.Heidi.Internal;
using Xunit;

public class HeidiModuleTest
{
    private readonly IHeidiConfigurationService _heidiConfigurationService;

    public HeidiModuleTest()
    {
        _heidiConfigurationService = A.Fake<IHeidiConfigurationService>();
    }

    [Fact]
    public void StartAsync_ShouldInitialize()
    {
        // arrange
        var task = Task.FromException(new Exception("throw by test, but doesn't matter"));
        A.CallTo(() => _heidiConfigurationService.InitializeAsync()).Returns(task);
        var sut = new HeidiModule(_heidiConfigurationService);

        // act
        Task result = sut.StartAsync();

        // assert
        A.CallTo(() => _heidiConfigurationService.InitializeAsync()).MustHaveHappenedOnceExactly();
        result.Should().Be(task);
    }

    [Fact]
    public async Task StopAsync_ShouldNotThrow()
    {
        // arrange
        var sut = new HeidiModule(_heidiConfigurationService);
        await sut.StartAsync();

        // act
        Func<Task> act = () => sut.StopAsync();
  
        // assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<HeidiModule> act = () => new HeidiModule(null!);
  
        // assert
        act.Should().Throw<ArgumentNullException>();
    }
}