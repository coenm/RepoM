namespace RepoM.App.Controls;

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using RepoM.App.ViewModels;

public partial class QuickFilterBar : UserControl
{
    private const string QUICK_FILTER_DATA_FORMAT = "QuickFilterVM";

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
        if (sender is FrameworkElement { DataContext: QuickFilterViewModel quickFilterViewModel, })
        {
            quickFilterViewModel.Toggle();
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
        if (sender is not FrameworkElement fe || e.LeftButton != MouseButtonState.Pressed || _draggingQfVm == null)
        {
            return;
        }

        Point currentPos = e.GetPosition(quickFilterItemsControl);
        if (Math.Abs(currentPos.X - _hamburgerDragStart.X) > 8 && !_isDraggingQuickFilter)
        {
            _isDraggingQuickFilter = true;

            var sourceBorder = FindParentBorder(fe);
            if (sourceBorder != null)
            {
                sourceBorder.Opacity = 0.25;
            }

            var data = new DataObject(QUICK_FILTER_DATA_FORMAT, _draggingQfVm);
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

    private void HamburgerButton_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement fe)
        {
            return;
        }

        fe.ReleaseMouseCapture();

        if (!_isDraggingQuickFilter && _draggingQfVm != null && !_draggingQfVm.IsBuiltIn)
        {
            OpenQuickFilterEditPopup(fe, _draggingQfVm);
        }

        _draggingQfVm = null;
        _isDraggingQuickFilter = false;
        e.Handled = true;
    }

    private void QuickFilter_Drop(object sender, DragEventArgs e)
    {
        HideAllDropIndicators();

        if (TryGetDropContext(sender, e, out DropContext dropContext) && ShouldSwapOrder(dropContext.Source, dropContext.Target))
        {
            SwapOrder(dropContext.Source, dropContext.Target);
        }

        e.Handled = true;
    }

    private void QuickFilter_DragOver(object sender, DragEventArgs e)
    {
        if (!TryGetDropContext(sender, e, out DropContext dropContext))
        {
            HideAllDropIndicators();
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        e.Effects = DragDropEffects.Move;
        UpdateDropIndicator(dropContext, e);

        e.Handled = true;
    }

    private void QuickFilter_DragLeave(object sender, DragEventArgs e)
    {
        HideAllDropIndicators();
        e.Handled = true;
    }

    private static bool TryGetDropContext(object sender, DragEventArgs e, out DropContext dropContext)
    {
        dropContext = null!;

        if (sender is not FrameworkElement fe
            || !TryGetDraggedQuickFilter(e, out QuickFilterViewModel sourceVm)
            || sourceVm.IsBuiltIn)
        {
            return false;
        }

        StackPanel? wrapper = FindAncestor<StackPanel>(fe, "qfDropWrapper");
        Border? targetBorder = FindChildByName<Border>(wrapper, "qfBorder");
        if (targetBorder?.DataContext is not QuickFilterViewModel targetVm
            || sourceVm.Id == targetVm.Id
            || targetVm.IsBuiltIn)
        {
            return false;
        }

        dropContext = new DropContext(wrapper!, targetBorder, sourceVm, targetVm);
        return true;
    }

    private static bool TryGetDraggedQuickFilter(DragEventArgs e, out QuickFilterViewModel sourceVm)
    {
        QuickFilterViewModel? draggedQuickFilter = e.Data.GetDataPresent(QUICK_FILTER_DATA_FORMAT)
            ? e.Data.GetData(QUICK_FILTER_DATA_FORMAT) as QuickFilterViewModel
            : null;

        sourceVm = draggedQuickFilter!;

        return sourceVm != null;
    }

    private bool ShouldSwapOrder(QuickFilterViewModel sourceVm, QuickFilterViewModel targetVm)
    {
        var sourceBefore = sourceVm.Order < targetVm.Order;
        return (sourceBefore && !_dropOnLeftSide) || (!sourceBefore && _dropOnLeftSide);
    }

    private static void SwapOrder(QuickFilterViewModel sourceVm, QuickFilterViewModel targetVm)
    {
        (sourceVm.Order, targetVm.Order) = (targetVm.Order, sourceVm.Order);
    }

    private void UpdateDropIndicator(DropContext dropContext, DragEventArgs e)
    {
        Point pos = e.GetPosition(dropContext.TargetBorder);
        var isLeft = pos.X < dropContext.TargetBorder.ActualWidth / 2;

        HideAllDropIndicators();

        var placeholderName = isLeft ? "dropPlaceholderLeft" : "dropPlaceholderRight";
        var placeholderTextName = isLeft ? "dropPlaceholderLeftText" : "dropPlaceholderRightText";
        Border? placeholder = FindChildByName<Border>(dropContext.Wrapper, placeholderName);
        TextBlock? placeholderText = FindChildByName<TextBlock>(dropContext.Wrapper, placeholderTextName);
        ShowDropIndicator(placeholder, placeholderText, dropContext.Source.DisplayLabel);
        _dropOnLeftSide = isLeft;
    }

    private static void ShowDropIndicator(Border? placeholder, TextBlock? placeholderText, string displayLabel)
    {
        if (placeholder == null || placeholderText == null)
        {
            return;
        }

        placeholderText.Text = displayLabel;
        placeholder.Visibility = Visibility.Visible;
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

            Border? left = FindChildByName<Border>(wrapper, "dropPlaceholderLeft");
            Border? right = FindChildByName<Border>(wrapper, "dropPlaceholderRight");
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

        if (!TryGetEditPopup(source, out Popup popup, out TextBox labelBox, out TextBox toolTipBox))
        {
            return;
        }

        labelBox.Text = qfVm.Label;
        toolTipBox.Text = qfVm.RawToolTip;
        popup.IsOpen = true;

        labelBox.Focus();
        labelBox.SelectAll();
    }

    private static bool TryGetEditPopup(FrameworkElement source, out Popup popup, out TextBox labelBox, out TextBox toolTipBox)
    {
        popup = null!;
        labelBox = null!;
        toolTipBox = null!;

        if (FindParentBorder(source)?.Child is not Grid grid)
        {
            return false;
        }

        Popup? popupElement = grid.Children.OfType<Popup>().FirstOrDefault();
        popup = popupElement!;
        if (popup.Child is not Border { Child: StackPanel stackPanel, })
        {
            popup = null!;
            return false;
        }

        TextBox[] textBoxes = stackPanel.Children.OfType<TextBox>().Take(2).ToArray();
        if (textBoxes.Length < 2)
        {
            popup = null!;
            return false;
        }

        labelBox = textBoxes[0];
        toolTipBox = textBoxes[1];
        return true;
    }

    private void QuickFilterPopupOk_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement fe || _editingQfVm == null)
        {
            return;
        }

        Popup? popup = FindPopupFromButton(fe);
        (TextBox? labelBox, TextBox? toolTipBox) = FindEditBoxesFromButton(fe);

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

    private void QuickFilterPopupCancel_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement fe)
        {
            return;
        }

        Popup? popup = FindPopupFromButton(fe);
        if (popup != null)
        {
            popup.IsOpen = false;
        }

        _editingQfVm = null;
    }

    private void QuickFilterPopupDelete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement fe || _editingQfVm == null)
        {
            return;
        }

        Popup? popup = FindPopupFromButton(fe);
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

    private static T? FindAncestor<T>(DependencyObject child, string? name = null) where T : FrameworkElement
    {
        DependencyObject? current = child;
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
            DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is T element && element.Name == name)
            {
                return element;
            }

            T? result = FindChildByName<T>(child, name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static Border? FindParentBorder(DependencyObject child)
    {
        DependencyObject? current = child;
        while (current != null)
        {
            if (current is Border { Name: "qfBorder", } border)
            {
                return border;
            }

            current = System.Windows.Media.VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private static Popup? FindPopupFromButton(DependencyObject element)
    {
        DependencyObject? current = element;
        while (current != null)
        {
            if (current is Popup popup)
            {
                return popup;
            }

            DependencyObject? parent = System.Windows.Media.VisualTreeHelper.GetParent(current);
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
        DependencyObject? current = element;
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

        TextBox[] textBoxes = stackPanel.Children.OfType<TextBox>().ToArray();
        return textBoxes.Length >= 2 ? (textBoxes[0], textBoxes[1]) : (null, null);
    }

    private sealed record DropContext(StackPanel Wrapper, Border TargetBorder, QuickFilterViewModel Source, QuickFilterViewModel Target);
}