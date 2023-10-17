namespace RepoM.ActionMenu.Core.Tests;

using System.IO.Abstractions;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RepoM.ActionMenu.Core;
using RepoM.ActionMenu.Core.PublicApi;
using SimpleInjector;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class ModuleTests
{
    private readonly Container _container = new();
    
    [Fact]
    public void RegisterServices_ShouldBeSuccessful_WhenExternalDependenciesAreRegistered()
    {
        // arrange
        RegisterExternals(_container);

        // act
        Bootstrapper.RegisterServices(_container);

        // assert
        // implicit, Verify throws when container is not valid.
        _container.Verify(VerificationOption.VerifyAndDiagnose);
    }
    
    [Fact]
    public void GetInstanceOf_IUserInterfaceActionMenuFactory_ShouldResolve()
    {
        // arrange
        RegisterExternals(_container);
        Bootstrapper.RegisterServices(_container);

        // act
        IUserInterfaceActionMenuFactory sut = _container.GetInstance<IUserInterfaceActionMenuFactory>();

        // assert
        sut.Should().NotBeNull();
    }
    
    private static void RegisterExternals(Container container)
    {
        container.RegisterSingleton(A.Dummy<ILogger>);
        container.RegisterSingleton(A.Dummy<IFileSystem>);
    }
}