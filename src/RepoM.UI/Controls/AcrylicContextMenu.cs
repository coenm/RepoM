namespace RepoM.UI.Controls;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RepoM.UI.Services;

public class AcrylicContextMenu : ContextMenu
{
    public bool UseFlatPopupStyle { get; set; }

    protected override void OnOpened(RoutedEventArgs e)
    {
        base.OnOpened(e);

        AcrylicHelper.EnableBlur(this);

        if (UseFlatPopupStyle)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                UpdateLayout();
                if (ItemContainerGenerator.ContainerFromIndex(0) is DependencyObject firstItem
                    && VisualTreeHelper.GetParent(firstItem) is Visual itemsHost)
                {
                    ApplyFlatPopupStyle(itemsHost);
                }
            }));
        }
    }

    internal void ApplyFlatPopupStyle(Visual itemsHost)
    {
        DependencyObject? parent = itemsHost;
        var hasSurface = false;
        while (parent != null)
        {
            // Theme shadow margins become a black strip around a native acrylic popup.
            if (parent is FrameworkElement element)
            {
                element.Margin = new Thickness(0);
                element.Effect = null;
            }

            if (parent is Border border)
            {
                border.Background = hasSurface ? Brushes.Transparent : Background;
                border.BorderBrush = hasSurface ? Brushes.Transparent : BorderBrush;
                border.BorderThickness = hasSurface ? new Thickness(0) : BorderThickness;
                border.Padding = hasSurface ? new Thickness(0) : new Thickness(0, 4, 0, 4);
                border.CornerRadius = new CornerRadius(2);
                hasSurface = true;
            }

            parent = VisualTreeHelper.GetParent(parent);
        }
    }
}