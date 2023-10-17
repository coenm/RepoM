namespace RepoM.ActionMenu.Core.Tests;

using FakeItEasy;
using Microsoft.Extensions.Logging;
using RepoM.ActionMenu.Core;
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
    
    private static void RegisterExternals(Container container)
    {
        IKeyTypeRegistration<IMenuAction> item = A.Fake<IKeyTypeRegistration<IMenuAction>>();
        A.CallTo(() => item.Tag).Returns("abc@1");
        A.CallTo(() => item.ConfigurationType).Returns(typeof(DummyMenuAction));
        container.Collection.AppendInstance<IKeyTypeRegistration<IMenuAction>>(item);

        container.RegisterSingleton(A.Dummy<ILogger>);
    }
}

file class DummyMenuAction : IMenuAction
{
    public string Type { get; } = "abc@1";

    public Predicate Active { get; } = true;
}