namespace RepoM.ActionMenu.Core.TestLib;

using System;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Core.Plugin;
using SimpleInjector;

internal class TestBootstrapper
{
    private readonly IPackageConfiguration _packageConfiguration;

    public TestBootstrapper(
        IPackageConfiguration packageConfiguration)
    {
        _packageConfiguration = packageConfiguration ?? throw new ArgumentNullException(nameof(packageConfiguration));
        FileSystem = new MockFileSystem();
        Container = new Container();
        Container.RegisterInstance<IFileSystem>(FileSystem);
        Container.RegisterInstance<ILogger>(NullLogger.Instance);
    }

    public Container Container { get; }

    public MockFileSystem FileSystem { get; }

    public void AddRootFile(string content, string path)
    {
        FileSystem.AddFile(path, new MockFileData(content));
    }

    public void RegisterActionMenuLibrary()
    {
        Bootstrapper.RegisterServices(Container);
    }

    public Task RegisterPlugin(IPackage package)
    {
        return package.RegisterServicesAsync(Container, _packageConfiguration);
    }

    public IUserInterfaceActionMenuFactory GetUserInterfaceActionMenu()
    {
        return Bootstrapper.GetUserInterfaceActionMenu(Container);
    }
}