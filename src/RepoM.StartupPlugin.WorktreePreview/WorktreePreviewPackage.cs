namespace RepoM.StartupPlugin.WorktreePreview;

using System.Threading.Tasks;
using RepoM.Core.Plugin;
using RepoM.StartupPlugin.Demo;
using RepoM.UI;
using SimpleInjector;

public sealed class WorktreePreviewPackage : IPackage
{
    public string Name => "WorktreePreview";

    public async Task RegisterServicesAsync(Container container, IPackageConfiguration packageConfiguration)
    {
        await new DemoPackage().RegisterServicesAsync(container, packageConfiguration).ConfigureAwait(false);
        bool allowOverriding = container.Options.AllowOverridingRegistrations;
        container.Options.AllowOverridingRegistrations = true;
        try
        {
            container.Register<IApplicationWindow, PreviewApplicationWindow>(Lifestyle.Singleton);
        }
        finally
        {
            container.Options.AllowOverridingRegistrations = allowOverriding;
        }
    }
}