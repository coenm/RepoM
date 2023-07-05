namespace RepoM.App.ViewModels;

using System;

public class ActionCheckableMenuItemViewModel : MenuItemViewModel
{
    private readonly Func<bool> _isSelectedFunc;
    private readonly Action _setKeyFunc;

    public ActionCheckableMenuItemViewModel(
        Func<bool> isSelectedFunc,
        Action setKeyFunc,
        string title)
    {
        _isSelectedFunc = isSelectedFunc ?? throw new ArgumentNullException(nameof(isSelectedFunc));
        _setKeyFunc = setKeyFunc ?? throw new ArgumentNullException(nameof(setKeyFunc));
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentNullException(nameof(title));
        }
        Header = title;
        
        IsCheckable = true;
    }

    public override bool IsChecked
    {
        get => _isSelectedFunc.Invoke();
        set
        {
            _ = value; // avoid warnings to use 'value' in setter.
            _setKeyFunc.Invoke();
        }
    }

    public void Poke()
    {
        OnPropertyChanged(nameof(IsChecked));
    }
}