namespace RepoM.ActionMenu.Core.TestLib;

using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using EasyTestFile;
using EasyTestFileXunit;
using FakeItEasy;
using RepoM.ActionMenu.Core;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.Repository;
using SimpleInjector;
using VerifyTests;
using Xunit;

[UsesEasyTestFile]
public abstract class IntegrationActionTestBase
{
    private protected readonly TestBootstrapper Bootstrapper;
    private const string DEFAULT_PATH = "C:\\RepositoriesV2.yaml";
    protected readonly EasyTestFileSettings TestFileSettings;
    protected readonly VerifySettings VerifySettings;

    protected IntegrationActionTestBase()
    {
        Repository = new Repository(@"C:\Repositories\work\RepoX")
            {
                CurrentBranch = "feature/123-my-new-ui-with-multiple-new-screens-so-this-has-a-long-branch-name",
                Branches = new string[1] { "develop", },
            };

        PackageConfiguration = A.Fake<IPackageConfiguration>();
        AppSettingsService = A.Fake<IAppSettingsService>();

        Bootstrapper = new TestBootstrapper(PackageConfiguration);
        Bootstrapper.RegisterActionMenuLibrary();

        Bootstrapper.Container.RegisterInstance(AppSettingsService);

        Bootstrapper.Container.Options.AllowOverridingRegistrations = true;
        Bootstrapper.Container.Options.EnableAutoVerification = false;

        TestFileSettings = new EasyTestFileSettings();
        TestFileSettings.UseExtension("yaml");

        VerifySettings = new VerifySettings();
        VerifySettings.DontScrubGuids();
        VerifySettings.ScrubMembersWithType<IRepository>();
    }

    protected IPackageConfiguration PackageConfiguration { get; }

    protected Repository Repository { get; }

    protected IAppSettingsService AppSettingsService { get; }

    protected Container Container => Bootstrapper.Container;

    protected MockFileSystem FileSystem => Bootstrapper.FileSystem;

    protected IUserInterfaceActionMenuFactory GetIUserInterfaceActionMenuFactory()
    {
        return Bootstrapper.GetUserInterfaceActionMenu();
    }

    [Fact]
    public virtual void ContainerVerify()
    {
        Container.Verify();
    }

    protected void AddRootFile(string content)
    {
        Bootstrapper.AddRootFile(content, DEFAULT_PATH);
    }

    protected async Task<IEnumerable<UserInterfaceRepositoryActionBase>> CreateMenuAsync()
    {
        IUserInterfaceActionMenuFactory factory = GetIUserInterfaceActionMenuFactory();
        return await factory.CreateMenuListAsync(Repository, DEFAULT_PATH);
    }
}


public abstract class IntegrationActionTestBase<T> : IntegrationActionTestBase where T : IPackage, new()
{
    protected IntegrationActionTestBase()
    {
        Bootstrapper.RegisterPlugin(new T());
    }
}