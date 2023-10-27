namespace RepoM.ActionMenu.Core.TestLib;

using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using RepoM.ActionMenu.Core;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.Repository;
using SimpleInjector;
using VerifyXunit;
using Xunit;

[UsesVerify]
public abstract class IntegrationActionTestBase<T> where T : IPackage, new()
{
    private readonly TestBootstrapper _bootstrapper;

    public IntegrationActionTestBase()
    {
        Repository = new Repository("C:\\repositories\\work");
        PackageConfiguration = A.Fake<IPackageConfiguration>();
        AppSettingsService = A.Fake<IAppSettingsService>();

        _bootstrapper = new TestBootstrapper(PackageConfiguration);
        _bootstrapper.RegisterActionMenuLibrary();
        _bootstrapper.RegisterPlugin(new T());

        _bootstrapper.Container.RegisterSingleton(A.Dummy<IRepositoryExpressionEvaluator>); // default
        _bootstrapper.Container.RegisterSingleton(A.Dummy<IActionToRepositoryActionMapper>); // default
        _bootstrapper.Container.RegisterInstance(AppSettingsService);

        _bootstrapper.Container.Options.AllowOverridingRegistrations = true;
        _bootstrapper.Container.Options.EnableAutoVerification = false;
    }

    protected IPackageConfiguration PackageConfiguration { get; }

    protected IRepository Repository { get; }

    protected IAppSettingsService AppSettingsService { get; }

    protected Container Container => _bootstrapper.Container;

    protected IUserInterfaceActionMenuFactory GetIUserInterfaceActionMenuFactory()
    {
        return _bootstrapper.GetUserInterfaceActionMenu();
    }

    [Fact]
    public void ContainerVerify()
    {
        Container.Verify();
    }

    protected void AddRootFile(string content, string path = "C:\\RepositoriesV2.yaml")
    {
        _bootstrapper.AddRootFile(content, path);
    }

    protected async Task<IEnumerable<UserInterfaceRepositoryActionBase>> CreateMenuAsync()
    {
        IUserInterfaceActionMenuFactory factory = GetIUserInterfaceActionMenuFactory();
        return await factory.CreateMenuAsync(Repository, "C:\\RepositoriesV2.yaml");
    }
}