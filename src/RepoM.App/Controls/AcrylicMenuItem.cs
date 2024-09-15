namespace RepoM.App.Controls;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RepoM.App.Services;

public class AcrylicMenuItem : Wpf.Ui.Controls.MenuItem
{
    //private static readonly Brush _solidColorBrush = new SolidColorBrush(Color.FromArgb(80, 0, 0, 0));

    //protected override void OnSubmenuOpened(RoutedEventArgs e)
    //{
    //    base.OnSubmenuOpened(e);

    //    Dispatcher.BeginInvoke((Action)BlurSubMenu);
    //}

    //private void BlurSubMenu()
    //{
    //    //DependencyObject firstSubItem = ItemContainerGenerator.ContainerFromIndex(0);

    //    //if (firstSubItem == null)
    //    //{
    //    //    return;
    //    //}

    //    //if (VisualTreeHelper.GetParent(firstSubItem) is not Visual container)
    //    //{
    //    //    return;
    //    //}

    //    //DependencyObject parent = container;
    //    //var borderIndex = 0;

    //    //while (parent != null)
    //    //{
    //    //    if (parent is Border b)
    //    //    {
    //    //        // only put color on the first border (transparent colors will add up otherwise)
    //    //        //b.Background = borderIndex == 0
    //    //        //    ? _solidColorBrush
    //    //        //    : Brushes.Transparent;

    //    //        borderIndex++;
    //    //    }

    //    //    parent = VisualTreeHelper.GetParent(parent);
    //    //}

    //    //AcrylicHelper.EnableBlur(container);
    //}
}
