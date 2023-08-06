namespace RepoM.Api.Tests;

using FakeItEasy;
using RepoM.Api.Plugins;
using System;
using System.IO.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

public class BootstrapperTests
{
    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<CoreBootstrapper> act1 = () => new CoreBootstrapper(A.Dummy<IPluginFinder>(), A.Dummy<IFileSystem>(), null!);
        Func<CoreBootstrapper> act2 = () => new CoreBootstrapper(A.Dummy<IPluginFinder>(), null!, A.Dummy<ILoggerFactory>());
        Func<CoreBootstrapper> act3 = () => new CoreBootstrapper(null!, A.Dummy<IFileSystem>(), A.Dummy<ILoggerFactory>());

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
        act3.Should().Throw<ArgumentNullException>();
    }
}