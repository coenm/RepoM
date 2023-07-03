namespace RepoM.App.Plugins;

using System;
using System.Diagnostics;

[DebuggerDisplay("{Name} (Enabled:{Enabled})")]
public sealed class PluginModel
{
    private readonly Action<string, bool> _updateAction;

    public PluginModel(bool enabled, Action<string, bool> updateAction)
    {
        _updateAction = updateAction ?? throw new ArgumentNullException(nameof(updateAction));
        Enabled = enabled;
    }

    public string Name { get; init; } = null!;

    public string Dll { get; init; } = null!;
    
    public bool Enabled { get; private set; }

    public bool Found { get; init; } = true;

    // this happends in UI thread.
    public void SetEnabled(bool enabled)
    {
        Enabled = enabled;
        _updateAction.Invoke(Dll, enabled);
    }
}