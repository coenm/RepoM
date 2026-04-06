namespace RepoM.App.Controls;

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using RepoM.Api.QuickFilter;
using RepoM.App.ViewModels;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;

public partial class QuickFilterBar : UserControl
{
    private const string QuickFilterDataFormat = "QuickFilterVM";

    private QuickFilterBarViewModel? _viewModel;
    private Point _hamburgerDragStart;
    private bool _isDraggingQuickFilter;
    private QuickFilterViewModel? _draggingQfVm;
    private bool _dropOnLeftSide;
    private QuickFilterViewModel? _editingQfVm;

    public QuickFilterBar()
    {
        InitializeComponent();
    }

    internal void Initialize(QuickFilterBarViewModel viewModel)
    {
        if (_viewModel != null)
        {
            return;
        }

        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = _viewModel;
    }

    private void QuickFilter_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is QuickFilterViewModel qfVm)
        {
            qfVm.Toggle();
            e.Handled = true;
        }
    }

    private void CombineModeToggle_Click(object sender, MouseButtonEventArgs e)
    {
        _viewModel?.ToggleCombineMode();
        e.Handled = true;
    }

    private void HamburgerButton_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe)
        {
            _hamburgerDragStart = e.GetPosition(quickFilterItemsControl);
            _isDraggingQuickFilter = false;
            _draggingQfVm = fe.DataContext as QuickFilterViewModel;
            fe.CaptureMouse();
            e.Handled = true;
        }
    }

    private void HamburgerButton_MouseMove(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement fe && e.LeftButton == MouseButtonState.Pressed && _draggingQfVm != null)
        {
            var currentPos = e.GetPosition(quickFilterItemsControl);
            if (Math.Abs(currentPos.X - _hamburgerDragStart.X) > 8 && !_isDraggingQuickFilter)
            {
                _isDraggingQuickFilter = true;

                var sourceBorder = FindParentBorder(fe);
                if (sourceBorder != null)
                {
                    sourceBorder.Opacity = 0.25;
                }

                var data = new DataObject(QuickFilterDataFormat, _draggingQfVm);
                fe.ReleaseMouseCapture();
                DragDrop.DoDragDrop(fe, data, DragDropEffects.Move);

                if (sourceBorder != null)
                {
                    sourceBorder.Opacity = 1.0;
                }

                _draggingQfVm = null;
            }

            e.Handled = true;
        }
    }

    private void HamburgerButton_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe)
        {
            fe.ReleaseMouseCapture();

            if (!_isDraggingQuickFilter && _draggingQfVm != null && !_draggingQfVm.IsBuiltIn)
            {
                OpenQuickFilterEditPopup(fe, _draggingQfVm);
            }

            _draggingQfVm = null;
            _isDraggingQuickFilter = false;
            e.Handled = true;
        }
    }

    private void QuickFilter_Drop(object sender, DragEventArgs e)
    {
        HideAllDropIndicators();

        if (sender is FrameworkElement fe
            && e.Data.GetDataPresent(QuickFilterDataFormat)
            && e.Data.GetData(QuickFilterDataFormat) is QuickFilterViewModel sourceVm
            && !sourceVm.IsBuiltIn)
        {
            var wrapper = FindAncestor<StackPanel>(fe, "qfDropWrapper");
            var targetBorder = FindChildByName<Border>(wrapper, "qfBorder");
            if (targetBorder?.DataContext is QuickFilterViewModel targetVm
                && sourceVm.Id != targetVm.Id
                && !targetVm.IsBuiltIn)
            {
                var sourceBefore = sourceVm.Order < targetVm.Order;
                if ((sourceBefore && !_dropOnLeftSide) || (!sourceBefore && _dropOnLeftSide))
                {
                    var sourceOrder = sourceVm.Order;
                    sourceVm.Order = targetVm.Order;
                    targetVm.Order = sourceOrder;
                }
            }
        }

        e.Handled = true;
    }

    private void QuickFilter_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(QuickFilterDataFormat)
            && e.Data.GetData(QuickFilterDataFormat) is QuickFilterViewModel sourceVm
            && sender is FrameworkElement fe)
        {
            e.Effects = DragDropEffects.Move;

            var wrapper = FindAncestor<StackPanel>(fe, "qfDropWrapper");
            if (wrapper != null)
            {
                var targetBorder = FindChildByName<Border>(wrapper, "qfBorder");
                if (targetBorder?.DataContext is QuickFilterViewModel targetVm
                    && sourceVm.Id != targetVm.Id
                    && !targetVm.IsBuiltIn)
                {
                    var pos = e.GetPosition(targetBorder);
                    var isLeft = pos.X < targetBorder.ActualWidth / 2;

                    HideAllDropIndicators();

                    var leftPlaceholder = FindChildByName<Border>(wrapper, "dropPlaceholderLeft");
                    var leftText = FindChildByName<TextBlock>(wrapper, "dropPlaceholderLeftText");
                    var rightPlaceholder = FindChildByName<Border>(wrapper, "dropPlaceholderRight");
                    var rightText = FindChildByName<TextBlock>(wrapper, "dropPlaceholderRightText");

                    if (isLeft && leftPlaceholder != null && leftText != null)
                    {
                        leftText.Text = sourceVm.DisplayLabel;
                        leftPlaceholder.Visibility = Visibility.Visible;
                        _dropOnLeftSide = true;
                    }
                    else if (!isLeft && rightPlaceholder != null && rightText != null)
                    {
                        rightText.Text = sourceVm.DisplayLabel;
                        rightPlaceholder.Visibility = Visibility.Visible;
                        _dropOnLeftSide = false;
                    }
                }
                else
                {
                    HideAllDropIndicators();
                }
            }
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    private void QuickFilter_DragLeave(object sender, DragEventArgs e)
    {
        HideAllDropIndicators();
        e.Handled = true;
    }

    private void HideAllDropIndicators()
    {
        foreach (var item in quickFilterItemsControl.Items)
        {
            if (quickFilterItemsControl.ItemContainerGenerator.ContainerFromItem(item) is not ContentPresenter container)
            {
                continue;
            }

            var wrapper = System.Windows.Media.VisualTreeHelper.GetChild(container, 0) as StackPanel;
            if (wrapper == null)
            {
                continue;
            }

            var left = FindChildByName<Border>(wrapper, "dropPlaceholderLeft");
            var right = FindChildByName<Border>(wrapper, "dropPlaceholderRight");
            if (left != null)
            {
                left.Visibility = Visibility.Collapsed;
            }

            if (right != null)
            {
                right.Visibility = Visibility.Collapsed;
            }
        }
    }

    private void OpenQuickFilterEditPopup(FrameworkElement source, QuickFilterViewModel qfVm)
    {
        _editingQfVm = qfVm;

        var border = FindParentBorder(source);
        if (border?.Child is not Grid grid)
        {
            return;
        }

        Popup? popup = null;
        TextBox? labelBox = null;
        TextBox? toolTipBox = null;

        foreach (var child in grid.Children)
        {
            if (child is Popup p)
            {
                popup = p;
            }
        }

        if (popup?.Child is Border popupBorder && popupBorder.Child is StackPanel stackPanel)
        {
            foreach (var child in stackPanel.Children)
            {
                if (child is TextBox textBox)
                {
                    if (labelBox == null)
                    {
                        labelBox = textBox;
                    }
                    else
                    {
                        toolTipBox = textBox;
                    }
                }
            }
        }

        if (popup == null || labelBox == null || toolTipBox == null)
        {
            return;
        }

        labelBox.Text = qfVm.Label;
        toolTipBox.Text = qfVm.RawToolTip;
        popup.IsOpen = true;

        labelBox.Focus();
        labelBox.SelectAll();
    }

    private void QuickFilterPopupOk_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && _editingQfVm != null)
        {
            var popup = FindPopupFromButton(fe);
            var (labelBox, toolTipBox) = FindEditBoxesFromButton(fe);

            if (labelBox != null && !string.IsNullOrWhiteSpace(labelBox.Text))
            {
                _editingQfVm.UpdateLabel(labelBox.Text.Trim());
            }

            if (toolTipBox != null)
            {
                _editingQfVm.UpdateToolTip(toolTipBox.Text.Trim());
            }

            if (popup != null)
            {
                popup.IsOpen = false;
            }

            _editingQfVm = null;
        }
    }

    private void QuickFilterPopupCancel_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe)
        {
            var popup = FindPopupFromButton(fe);
            if (popup != null)
            {
                popup.IsOpen = false;
            }

            _editingQfVm = null;
        }
    }

    private void QuickFilterPopupDelete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && _editingQfVm != null)
        {
            var popup = FindPopupFromButton(fe);
            if (popup != null)
            {
                popup.IsOpen = false;
            }

            if (!_editingQfVm.IsBuiltIn)
            {
                _viewModel?.Remove(_editingQfVm.Id);
            }

            _editingQfVm = null;
        }
    }

    private static T? FindAncestor<T>(DependencyObject child, string? name = null) where T : FrameworkElement
    {
        var current = child;
        while (current != null)
        {
            if (current is T element && (name == null || element.Name == name))
            {
                return element;
            }

            current = System.Windows.Media.VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private static T? FindChildByName<T>(DependencyObject? parent, string name) where T : FrameworkElement
    {
        if (parent == null)
        {
            return null;
        }

        var count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < count; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is T element && element.Name == name)
            {
                return element;
            }

            var result = FindChildByName<T>(child, name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static Border? FindParentBorder(DependencyObject child)
    {
        var current = child;
        while (current != null)
        {
            if (current is Border border && border.Name == "qfBorder")
            {
                return border;
            }

            current = System.Windows.Media.VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private static Popup? FindPopupFromButton(DependencyObject element)
    {
        var current = element;
        while (current != null)
        {
            if (current is Popup popup)
            {
                return popup;
            }

            var parent = System.Windows.Media.VisualTreeHelper.GetParent(current);
            if (parent == null && current is FrameworkElement frameworkElement)
            {
                parent = frameworkElement.Parent;
            }

            current = parent;
        }

        return null;
    }

    private static (TextBox? labelBox, TextBox? toolTipBox) FindEditBoxesFromButton(DependencyObject element)
    {
        var current = element;
        StackPanel? stackPanel = null;
        while (current != null)
        {
            if (current is StackPanel panel && panel.Children.OfType<TextBox>().Any())
            {
                stackPanel = panel;
                break;
            }

            var parent = System.Windows.Media.VisualTreeHelper.GetParent(current);
            if (parent == null && current is FrameworkElement frameworkElement)
            {
                parent = frameworkElement.Parent;
            }

            current = parent;
        }

        if (stackPanel == null)
        {
            return (null, null);
        }

        var textBoxes = stackPanel.Children.OfType<TextBox>().ToArray();
        return textBoxes.Length >= 2 ? (textBoxes[0], textBoxes[1]) : (null, null);
    }
}