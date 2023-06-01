namespace RepoM.Plugin.SonarCloud.Tests;

using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Xunit;

public class SonarCloudModuleTest
{
    private readonly ISonarCloudFavoriteService _sonarCloudFavoriteService;

    public SonarCloudModuleTest()
    {
        _sonarCloudFavoriteService = A.Fake<ISonarCloudFavoriteService>();
    }

    [Fact]
    public void StartAsync_ShouldInitialize()
    {
        // arrange
        var task = Task.FromException(new Exception("throw by test, but doesn't matter"));
        A.CallTo(() => _sonarCloudFavoriteService.InitializeAsync()).Returns(task);
        var sut = new SonarCloudModule(_sonarCloudFavoriteService);

        // act
        Task result = sut.StartAsync();

        // assert
        A.CallTo(() => _sonarCloudFavoriteService.InitializeAsync()).MustHaveHappenedOnceExactly();
        result.Should().Be(task);
    }

    [Fact]
    public async Task StopAsync_ShouldNotThrow()
    {
        // arrange
        var sut = new SonarCloudModule(_sonarCloudFavoriteService);
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
        Func<SonarCloudModule> act = () => new SonarCloudModule(null!);
  
        // assert
        act.Should().Throw<ArgumentNullException>();
    }
}