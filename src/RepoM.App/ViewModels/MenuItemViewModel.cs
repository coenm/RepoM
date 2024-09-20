namespace RepoM.App.ViewModels;

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/contextmenu-styles-and-templates?view=netframeworkdesktop-4.8

public class MenuItemViewModel : INotifyPropertyChanged
{
    public string Header { get; set; } = string.Empty;

    public bool IsCheckable { get; set; }

    public virtual bool IsChecked { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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
