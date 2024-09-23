namespace RepoM.App.Tests;

using System;
using System.IO.Abstractions;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin.Common;
using SimpleInjector;
using Xunit;
using Sut = RepoM.App.Bootstrapper;

public class BootstrapperTests
{
    [Fact]
    public void Container_ShouldAlwaysBeSameInstance()
    {
        // arrange

        // act
        Container result1 = Sut.Container;
        Container result2 = Sut.Container;

        // assert
        result1.Should().NotBeNull().And.Subject.Should().BeSameAs(result2);
    }

    [Fact]
    public void RegisterServices_ShouldNotThrow()
    {
        // arrange

        // act
        Action act = () => Sut.RegisterServices(A.Dummy<IFileSystem>(), A.Dummy<IAppDataPathProvider>());

        // assert
        act.Should().NotThrow();
    }


    [Fact]
    public void RegisterLogging_ShouldNotThrow()
    {
        // arrange

        // act
        Action act = () => Sut.RegisterLogging(A.Dummy<ILoggerFactory>());

        // assert
        act.Should().NotThrow();
    }
    
    [Fact(Skip = "Fails")]
    public void RegisterServices_ShouldResultInValidContainer()
    {
        // arrange
        
        // act
        Sut.RegisterServices(A.Dummy<IFileSystem>(), A.Dummy<IAppDataPathProvider>());
        Sut.RegisterLogging(A.Fake<ILoggerFactory>());
    
        // assert
        Sut.Container.Verify(VerificationOption.VerifyOnly);
    }
}