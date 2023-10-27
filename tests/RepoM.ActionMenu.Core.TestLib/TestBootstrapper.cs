namespace RepoM.ActionMenu.Core.TestLib
{
    using System;
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using RepoM.Core.Plugin;
    using SimpleInjector;

    public class TestBootstrapper
    {
        private readonly IPackageConfiguration _packageConfiguration;

        public TestBootstrapper(
            IPackageConfiguration packageConfiguration,
            IFileSystem fileSystem)
        {
            _packageConfiguration = packageConfiguration ?? throw new ArgumentNullException(nameof(packageConfiguration));
            _ = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            Container = new Container();
            Container.RegisterInstance(fileSystem);
        }

        public Container Container { get; }

        public void RegisterActionMenuLibrary()
        {
            Bootstrapper.RegisterServices(Container);
        }

        public Task RegisterPlugin(IPackage package)
        {
            return package.RegisterServicesAsync(Container, _packageConfiguration);
        }
    }
}
