namespace RepoM.StartupPlugin.Demo;

using System;
using RepoM.Core.Plugin.Common;

public sealed class DemoPresentationDecorator : IApplicationPresentation
{
    public DemoPresentationDecorator(IApplicationPresentation decoratee)
    {
        ArgumentNullException.ThrowIfNull(decoratee);
    }

    public string WindowTitle => "RepoM - Demo";

    public bool UseTray => false;
}
