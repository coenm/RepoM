namespace RepoM.App.Controls;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;

internal sealed class RowAnimationController
{
    private readonly HashSet<FrameworkElement> _rows = [];

    public void Animate(FrameworkElement row, double fromHeight, bool fadeIn = false)
    {
        double targetHeight = row.ActualHeight;
        if (Math.Abs(fromHeight - targetHeight) < 0.1)
        {
            return;
        }

        var duration = TimeSpan.FromMilliseconds(180);
        var height = new DoubleAnimation(fromHeight, targetHeight, duration)
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };
        height.Completed += (_, _) => Finish(row);
        _rows.Add(row);
        row.Unloaded += Row_Unloaded;
        row.DataContextChanged += Row_DataContextChanged;
        row.Height = fromHeight;
        row.BeginAnimation(FrameworkElement.HeightProperty, height);
        if (fadeIn)
        {
            row.Opacity = 0;
            row.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, 1, duration));
        }
    }

    public void FinishAll()
    {
        foreach (FrameworkElement row in _rows.ToArray())
        {
            Finish(row);
        }
    }

    public void Finish(FrameworkElement row)
    {
        if (!_rows.Remove(row))
        {
            return;
        }

        row.Unloaded -= Row_Unloaded;
        row.DataContextChanged -= Row_DataContextChanged;
        row.BeginAnimation(FrameworkElement.HeightProperty, null);
        row.BeginAnimation(UIElement.OpacityProperty, null);
        row.ClearValue(FrameworkElement.HeightProperty);
        row.ClearValue(UIElement.OpacityProperty);
    }

    private void Row_Unloaded(object sender, RoutedEventArgs e) => Finish((FrameworkElement)sender);

    private void Row_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) => Finish((FrameworkElement)sender);
}
