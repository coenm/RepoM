namespace RepoM.App.ViewModels;

using System;
using RepoM.App.Plugins;

public class PluginViewModel : MenuItemViewModel
{
    private readonly PluginModel _model;

    public PluginViewModel(PluginModel model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        Header = model.Name;
        IsCheckable = model.Found;
    }

    public override bool IsChecked
    {
        get => _model.Enabled;
        set => _model.SetEnabled(value);
    }
}