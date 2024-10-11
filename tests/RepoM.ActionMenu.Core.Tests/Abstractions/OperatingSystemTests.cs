namespace RepoM.ActionMenu.Core.Tests.Abstractions;

using System;
using System.IO.Abstractions;
using FakeItEasy;
using FluentAssertions;
using RepoM.ActionMenu.Core.Abstractions;
using Xunit;
using OperatingSystem = RepoM.ActionMenu.Core.Abstractions.OperatingSystem;

public class OperatingSystemTests
{
    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        System.Func<OperatingSystem> act1 = () => new OperatingSystem(A.Dummy<IFileSystem>(), null!);
        System.Func<OperatingSystem> act2 = () => new OperatingSystem(null!, A.Dummy<IEnvironment>());

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Ctor_ShouldExposeFileSystemUsingProperty()
    {
        // arrange
        var fs = A.Dummy<IFileSystem>();
        
        // act
        var sut = new OperatingSystem(fs, A.Dummy<IEnvironment>());

        // assert
        sut.FileSystem.Should().BeSameAs(fs);
    }

    [Fact]
    public void Ctor_ShouldExposeEnvironmentUsingProperty()
    {
        // arrange
        var env = A.Dummy<IEnvironment>();
        
        // act
        var sut = new OperatingSystem(A.Dummy<IFileSystem>(), env);

        // assert
        sut.Environment.Should().BeSameAs(env);
    }
}