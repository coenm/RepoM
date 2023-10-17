namespace RepoM.ActionMenu.Core.Tests;

using System.IO.Abstractions;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RepoM.ActionMenu.Core;
using RepoM.ActionMenu.Core.PublicApi;
using RepoM.ActionMenu.Interface.Scriban;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
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
        IKeyTypeRegistration<IMenuAction> keyTypeDeserializationItem = A.Fake<IKeyTypeRegistration<IMenuAction>>();
        A.CallTo(() => keyTypeDeserializationItem.Tag).Returns("abc@1");
        A.CallTo(() => keyTypeDeserializationItem.ConfigurationType).Returns(typeof(DummyMenuAction));
        container.Collection.AppendInstance<IKeyTypeRegistration<IMenuAction>>(keyTypeDeserializationItem);

        ITemplateContextRegistration functionRegistrationItem = A.Fake<ITemplateContextRegistration>();
        container.Collection.AppendInstance<ITemplateContextRegistration>(functionRegistrationItem);

        IActionToRepositoryActionMapper repositoryActionMapper = A.Fake<IActionToRepositoryActionMapper>();
        container.Collection.AppendInstance<IActionToRepositoryActionMapper>(repositoryActionMapper);

        container.RegisterSingleton(A.Dummy<ILogger>);
        container.RegisterSingleton(A.Dummy<IFileSystem>);
    }
}

file class DummyMenuAction : IMenuAction
{
    public string Type { get; } = "abc@1";

    public Predicate Active { get; } = true;
}