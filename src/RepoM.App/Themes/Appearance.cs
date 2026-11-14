namespace RepoM.App.Themes;

using System.Windows;

public static class Appearance
{
    public static readonly DependencyProperty EnableAnimationsProperty = DependencyProperty.RegisterAttached(
        "EnableAnimations", typeof(bool), typeof(Appearance),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));

    public static bool GetEnableAnimations(DependencyObject element) => (bool)element.GetValue(EnableAnimationsProperty);

    public static void SetEnableAnimations(DependencyObject element, bool value) => element.SetValue(EnableAnimationsProperty, value);
}
