namespace RepoM.App.Converters;

using System;
using System.Globalization;
using System.Windows.Data;

/// <summary>
/// Converts a UTC DateTime to local DateTime.
/// </summary>
public class UtcToLocalDateTimeConverter : IValueConverter
{
    /// <summary>
    /// Converts a UTC DateTime to local DateTime.
    /// </summary>
    /// <param name="value">The UTC DateTime value to convert.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>A local DateTime value.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        DateTime.SpecifyKind(DateTime.Parse(value?.ToString() ?? string.Empty, CultureInfo.InvariantCulture), DateTimeKind.Utc).ToLocalTime();

    /// <summary>
    /// Converts a value back. This method is not implemented.
    /// </summary>
    /// <param name="value">The value that is produced by the binding target.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>A converted value.</returns>
    /// <exception cref="NotImplementedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
