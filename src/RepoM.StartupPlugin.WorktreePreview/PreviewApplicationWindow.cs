namespace RepoM.StartupPlugin.WorktreePreview;

using System.Windows;
using RepoM.UI;

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