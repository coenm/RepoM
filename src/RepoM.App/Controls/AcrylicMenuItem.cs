namespace RepoM.App.Controls;

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.App.Services;

public class AcrylicMenuItem : MenuItem
{
    private RoutedEventHandler? _clickEvtHandler;
    private RoutedEventHandler? _subMenuOpenedEventHandler;
    private string _header = string.Empty;
    private static readonly Brush _solidColorBrush = new SolidColorBrush(Color.FromArgb(80, 0, 0, 0));
    private bool _isEnabled = default;

    public Task<UserInterfaceRepositoryActionBase[]> DataTask { get; private set; } = Task.FromResult(Array.Empty<UserInterfaceRepositoryActionBase>());

    public void SetClick(RoutedEventHandler routedEventHandler)
    {
        ClearClick();
        Click += routedEventHandler;
        _clickEvtHandler = routedEventHandler;
    }

    public void ClearClick()
    {
        if (_clickEvtHandler == null)
        {
            return;
        }

        Click -= _clickEvtHandler;
        _clickEvtHandler = null;
    }

    public void SetSubMenuOpened(RoutedEventHandler routedEventHandler)
    {
        ClearSubMenuOpened();
        SubmenuOpened += routedEventHandler;
        _subMenuOpenedEventHandler = routedEventHandler;
    }

    public void ClearSubMenuOpened()
    {
        if (_subMenuOpenedEventHandler == null)
        {
            return;
        }

        SubmenuOpened -= _subMenuOpenedEventHandler;
        _subMenuOpenedEventHandler = null;
    }

    public void ClearItems()
    {
        if (Items.Count == 0)
        {
            return;
        }

        Items.Clear();
    }

    public void SetHeader(string name)
    {
        if (name.Equals(_header))
        {
            return;
        }

        Header = name;
        _header = name;
    }

    public void SetEnabled(bool repositoryActionCanExecute)
    {
        if (_isEnabled == repositoryActionCanExecute)
        {
            return;
        }

        IsEnabled = _isEnabled = repositoryActionCanExecute;
    }

    public void LoadData(DeferredSubActionsUserInterfaceRepositoryAction deferredRepositoryAction)
    {
        ClearData();
        DataTask = Task.Run(async () => await deferredRepositoryAction.GetAsync());
    }

    public void ClearData()
    {
        try
        {
            DataTask.Dispose();
        }
        catch (Exception)
        {
            // swallow
        }

        DataTask = Task.FromResult(Array.Empty<UserInterfaceRepositoryActionBase>());
    }

    protected override void OnSubmenuOpened(RoutedEventArgs e)
    {
        base.OnSubmenuOpened(e);

        Dispatcher.BeginInvoke((Action)BlurSubMenu);
    }

    private void BlurSubMenu()
    {
        DependencyObject firstSubItem = ItemContainerGenerator.ContainerFromIndex(0);

        if (firstSubItem == null)
        {
            return;
        }

        if (VisualTreeHelper.GetParent(firstSubItem) is not Visual container)
        {
            return;
        }

        DependencyObject? parent = container;
        var borderIndex = 0;

        while (parent != null)
        {
            if (parent is Border b)
            {
                // only put color on the first border (transparent colors will add up otherwise)
                b.Background = borderIndex == 0
                    ? _solidColorBrush
                    : Brushes.Transparent;

                borderIndex++;
            }

            parent = VisualTreeHelper.GetParent(parent);
        }

        AcrylicHelper.EnableBlur(container);
    }

    
}