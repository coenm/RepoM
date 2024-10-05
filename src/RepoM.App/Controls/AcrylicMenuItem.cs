namespace RepoM.App.Controls;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RepoM.App.Services;

public class AcrylicMenuItem : MenuItem
{
    private RoutedEventHandler? _clickEvtHandler;
    private RoutedEventHandler? _subMenuOpenedEventHandler;
    private static readonly Brush _solidColorBrush = new SolidColorBrush(Color.FromArgb(80, 0, 0, 0));

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

    public void SoftReset()
    {
        ClearClick();
        ClearSubMenuOpened();
    }

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
}