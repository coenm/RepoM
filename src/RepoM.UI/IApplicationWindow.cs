namespace RepoM.UI;

using System.Windows;

/// <summary>The window and UI lifecycle activated by the host.</summary>
public interface IApplicationWindow
{
    Window Window { get; }

    void SetReady();

    void ShowAndActivate();

    void Shutdown();
}
