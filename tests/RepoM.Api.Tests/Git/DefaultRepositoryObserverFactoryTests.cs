namespace RepoM.Api.Tests.Git;

using System;
using System.IO.Abstractions;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RepoM.Api.Git;
using Xunit;

public class DefaultRepositoryObserverFactoryTests
{
    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<DefaultRepositoryObserverFactory> act1 = () => new DefaultRepositoryObserverFactory(A.Dummy<ILoggerFactory>(), null!);
        Func<DefaultRepositoryObserverFactory> act2 = () => new DefaultRepositoryObserverFactory(null!, A.Dummy<IFileSystem>());

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_ShouldReturnInstanceOfDefaultRepositoryObserver()
    {
        // arrange
        ILoggerFactory loggerFactory = A.Fake<ILoggerFactory>();
        IFileSystem fileSystem = A.Fake<IFileSystem>();
        var sut = new DefaultRepositoryObserverFactory(loggerFactory, fileSystem);

        // act
        IRepositoryObserver result = sut.Create();

        // assert
        _  = result.Should().NotBeNull().And.BeOfType<DefaultRepositoryObserver>();
    }
}