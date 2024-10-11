namespace RepoM.ActionMenu.Core.Tests.Abstractions;

using System;
using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;
using RepoM.ActionMenu.Core.Abstractions;
using Xunit;
using IEnvironment = RepoM.ActionMenu.Core.Abstractions.IEnvironment;

public class EnvironmentsCacheDecoratorTests
{
    private readonly IEnvironment _decoratee = A.Fake<IEnvironment>();
    private readonly EnvironmentsCacheDecorator _sut;

    public EnvironmentsCacheDecoratorTests()
    {
        _sut = new EnvironmentsCacheDecorator(_decoratee);
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<EnvironmentsCacheDecorator> act1 = () => new EnvironmentsCacheDecorator(null!);

        // assert
        act1.Should().Throw<ArgumentNullException>();
    }
    
    [Fact]
    public void Ctor_ShouldNotCallDecoratee()
    {
        // arrange

        // act
        _ = new EnvironmentsCacheDecorator(_decoratee);

        // assert
        A.CallTo(_decoratee).MustNotHaveHappened();
    }

    [Fact]
    public void GetEnvironmentVariables_ShouldForwardToDecoratee_WhenFirstCall()
    {
        // arrange

        // act
        _ = _sut.GetEnvironmentVariables();

        // assert
        A.CallTo(() => _decoratee.GetEnvironmentVariables()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void GetEnvironmentVariables_ShouldReturnDecorateeResult_WhenFirstCall()
    {
        // arrange
        var envVars = new Dictionary<string, string>
            {
                { "dummy", Guid.NewGuid().ToString() },
            };
        
        A.CallTo(() => _decoratee.GetEnvironmentVariables()).Returns(envVars);

        // act
        var result = _sut.GetEnvironmentVariables();

        // assert
        result.Should().BeSameAs(envVars);
    }

    [Fact]
    public void GetEnvironmentVariables_ShouldReturnCachedValue_WhenCalledForSecondTime()
    {
        // arrange
        var envVars = new Dictionary<string, string>
            {
                { "dummy", Guid.NewGuid().ToString() },
            };
        
        A.CallTo(() => _decoratee.GetEnvironmentVariables()).Returns(envVars);
        Dictionary<string, string> result1 = _sut.GetEnvironmentVariables();

        Fake.ClearRecordedCalls(_decoratee);

        // act
        Dictionary<string, string> result2 = _sut.GetEnvironmentVariables();

        // assert
        result2.Should().BeSameAs(result1);
        A.CallTo(_decoratee).MustNotHaveHappened();
    }
}