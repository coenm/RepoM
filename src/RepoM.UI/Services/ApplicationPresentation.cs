namespace RepoM.UI.Services;

using RepoM.Core.Plugin.Common;

internal sealed class ApplicationPresentation : IApplicationPresentation
{
    public string WindowTitle => "RepoM";

    public bool UseTray => true;
}
