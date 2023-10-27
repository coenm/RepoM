namespace RepoM.ActionMenu.Core.TestLib
{
    using RepoM.Core.Plugin;
    using SimpleInjector;

    public class TestBootstrapper
    {
        public TestBootstrapper()
        {
            Container = new Container();
        }

        public Container Container { get; }

        public void RegisterActionMenuLibrary()
        {
            Bootstrapper.RegisterServices(Container);
        }

        public void RegisterPlugin(IPackage package)
        {
            package.RegisterServicesAsync(Container, null!);
        }
    }
}
