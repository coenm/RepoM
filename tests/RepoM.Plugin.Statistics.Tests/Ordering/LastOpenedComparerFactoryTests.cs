namespace RepoM.Plugin.Statistics.Tests.Ordering;

using System;
using FakeItEasy;
using FluentAssertions;
using RepoM.Core.Plugin.RepositoryOrdering;
using RepoM.Plugin.Statistics.Ordering;
using Xunit;

public class LastOpenedComparerFactoryTests
{
    private readonly IStatisticsService _service;

    public LastOpenedComparerFactoryTests()
    {
        _service = A.Fake<IStatisticsService>();
    }

    [Fact]
    public void Ctor_ShouldThrown_WhenArgumentIsNull()
    {
        // arrange

        // act
        Action act1 = () => _ = new LastOpenedComparerFactory(null!);

        // assert
        act1.Should().ThrowExactly<ArgumentNullException>();
    }


    [Fact]
    public void Create_ShouldCreateLastOpenedComparer()
    {
        // arrange
        var configuration = new LastOpenedConfigurationV1
            {
                Weight = 123,
            };
        var sut = new LastOpenedComparerFactory(_service);

        // act
        IRepositoryComparer result = sut.Create(configuration);

        // assert
        result.Should().NotBeNull().And.BeOfType<LastOpenedComparer>();
    }
}