namespace RepoM.Plugin.WorktreePreview;

using System.Threading.Tasks;
using System.Windows;
using RepoM.Core.Plugin;
using RepoM.Plugin.Demo;
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

public sealed class PreviewApplicationWindow : IApplicationWindow
{
    public Window Window { get; } = new WorktreePreviewWindow();

    public void SetReady()
    {
    }

    public void ShowAndActivate()
    {
        Window.Show();
        Window.Activate();
    }

    public void Shutdown() => Window.Close();
}
