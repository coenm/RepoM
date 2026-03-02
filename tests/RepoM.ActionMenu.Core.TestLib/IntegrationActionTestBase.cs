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
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Repositories.Adapters;
using RepoM.Core.Repositories.Model;
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
        RepositoryInfo = new RepositoryInfo
        {
            Path = @"C:\Repositories\work\RepoM",
            SafePath = "C:/Repositories/work/RepoM",
            WindowsPath = @"C:\Repositories\work\RepoM",
            LinuxPath = "C:/Repositories/work/RepoM",
            Location = "C:/Repositories/work/RepoM",
            Name = "RepoM",
            CurrentBranch = "feature/123-my-new-ui-with-multiple-new-screens-so-this-has-a-long-branch-name",
            Branches = ["develop",],
            Remotes =
            {
                new Remote("origin", "https://www.github.com/coenm/RepoM/"),
                new Remote("fork1", "https://www.github.com/coenm/RepoM-Fork1/"),
                new Remote("fork2", "https://www.github.com/coenm/RepoM-Fork2/"),
                new Remote("ssh-fork", "ssh://user@github.com/coenm/RepoM-Fork3/"),
            },
        };
        Repository = new RepositoryInfoAdapter(RepositoryInfo);

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

    protected RepositoryInfo RepositoryInfo { get; }

    protected IRepository Repository { get; }

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
