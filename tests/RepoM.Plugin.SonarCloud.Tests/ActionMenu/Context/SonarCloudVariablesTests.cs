namespace RepoM.Plugin.SonarCloud.Tests.ActionMenu.Context;

using System;
using FakeItEasy;
using FluentAssertions;
using RepoM.Plugin.SonarCloud.ActionMenu.Context;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class SonarCloudVariablesTests
{
    private readonly ISonarCloudFavoriteService _service;
    private readonly SonarCloudVariables _sut;

    public SonarCloudVariablesTests()
    {
        _service = A.Fake<ISonarCloudFavoriteService>();
        _sut = new SonarCloudVariables(_service);
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<SonarCloudVariables> act1 = () => new SonarCloudVariables(null!);

        // assert
        act1.Should().Throw<ArgumentNullException>();
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void IsFavorite_ShouldReturnFalse_WhenProjectIdIsNullOrEmpty(string? projectId)
    {
        // arrange

        // act
        bool result = _sut.IsFavorite(projectId!);

        // assert
        result.Should().BeFalse();
        A.CallTo(_service).MustNotHaveHappened();
    }

    [Theory]
    [InlineData("a", true)]
    [InlineData("b", false)]
    public void IsFavorite_ShouldReturnServiceResponse_WhenProjectIdIsNotNullOrEmpty(string projectId, bool serviceResponse)
    {
        // arrange
        A.CallTo(() => _service.IsFavorite(projectId)).Returns(serviceResponse);

        // act
        bool result = _sut.IsFavorite(projectId);

        // assert
        result.Should().Be(serviceResponse);
        A.CallTo(_service).MustHaveHappenedOnceExactly();
        A.CallTo(() => _service.IsFavorite(projectId)).MustHaveHappenedOnceExactly();
    }
}