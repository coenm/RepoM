namespace RepoM.Core.Plugin.Common;

/// <summary>Controls whether the application behaves as a tray utility or a standalone window.</summary>
public interface IApplicationPresentation
{
    string WindowTitle { get; }

    bool UseTray { get; }
}
