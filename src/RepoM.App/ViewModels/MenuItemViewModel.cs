namespace RepoM.App.ViewModels;

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

// https://stackoverflow.com/questions/5912687/styling-contextmenu-and-contextmenu-items
// https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/contextmenu-styles-and-templates?view=netframeworkdesktop-4.8
// https://itecnote.com/tecnote/wpf-how-to-bind-an-observablecollection-of-viewmodels-to-a-menuitem/
public class MenuItemViewModel : INotifyPropertyChanged
{
    public string Header { get; set; } = string.Empty;

    public bool IsCheckable { get; set; }

    public virtual bool IsChecked { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}