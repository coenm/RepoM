namespace RepoM.ActionMenu.Core.TestLib
{
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
    }
}
