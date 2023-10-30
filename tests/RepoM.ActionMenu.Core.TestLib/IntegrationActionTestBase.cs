namespace RepoM.ActionMenu.Core.TestLib;

using System.Collections.Generic;
using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
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
using VerifyTests;
using VerifyXunit;
using Xunit;

[UsesVerify]
[UsesEasyTestFile]
public abstract class IntegrationActionTestBase<T> where T : IPackage, new()
{
    private readonly TestBootstrapper _bootstrapper;
    protected const string DEFAULT_PATH = "C:\\RepositoriesV2.yaml";
    protected readonly EasyTestFileSettings TestFileSettings;
    protected readonly VerifySettings VerifySettings;

    public IntegrationActionTestBase()
    {
        Repository = new Repository("C:\\Repositories\\work");
        Repository.CurrentBranch = "feature/123-my-new-ui-with-multiple-new-screens-so-this-has-a-long-branch-name";
        Repository.Branches = new string[1] { "develop", };

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

        TestFileSettings = new EasyTestFileSettings();
        TestFileSettings.UseExtension("yaml");

        VerifySettings = new VerifySettings();
        VerifySettings.DontScrubGuids();
        VerifySettings.ScrubMembersWithType<IRepository>();
    }

    protected IPackageConfiguration PackageConfiguration { get; }

    protected Repository Repository { get; }

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

    protected void AddRootFile(string content)
    {
        _bootstrapper.AddRootFile(content, DEFAULT_PATH);
    }

    protected async Task<IEnumerable<UserInterfaceRepositoryActionBase>> CreateMenuAsync()
    {
        IUserInterfaceActionMenuFactory factory = GetIUserInterfaceActionMenuFactory();
        return await factory.CreateMenuAsync(Repository, DEFAULT_PATH);
    }
}