namespace RepoM.Plugin.SonarCloud.Tests;

using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Xunit;

public class SonarCloudFavoriteServiceTest
{
    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentIsNull()
    {
        // arrange

        // act
        Func<SonarCloudFavoriteService> act = () => _ = new SonarCloudFavoriteService(null!);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task InitializeAsync_ShouldNotInitialize_WhenPatInvalid(string? pat)
    {
        // arrange
        ISonarCloudConfiguration appSettingsService = A.Fake<ISonarCloudConfiguration>();
        A.CallTo(() => appSettingsService.PersonalAccessToken).Returns(pat!);
        var sut = new SonarCloudFavoriteService(appSettingsService);

        // act
        await sut.InitializeAsync();

        // assert
        sut.IsInitialized.Should().BeFalse();
    }

    [Fact]
    public void SetFavorite_ShouldReturn_WhenNotInitialized()
    {
        // arrange
        ISonarCloudConfiguration appSettingsService = A.Fake<ISonarCloudConfiguration>();
        var sut = new SonarCloudFavoriteService(appSettingsService);

        // assume
        sut.IsInitialized.Should().BeFalse();

        // act
        Func<Task> act = async () => await sut.SetFavorite("dummy");

        // assert
        _ = act.Should().NotThrowAsync();
    }

    [Fact]
    public void IsFavorite_ShouldReturnFalse_WhenNotInitialized()
    {
        // arrange
        ISonarCloudConfiguration appSettingsService = A.Fake<ISonarCloudConfiguration>();
        var sut = new SonarCloudFavoriteService(appSettingsService);

        // assume
        sut.IsInitialized.Should().BeFalse();

        // act
        var result = sut.IsFavorite("dummy");

        // assert
        result.Should().BeFalse();
    }
}