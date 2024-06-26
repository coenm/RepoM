namespace RepoM.App.Converters;

using System;
using System.Globalization;
using System.Windows.Data;

public class UtcToLocalDateTimeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return DateTime.SpecifyKind(DateTime.Parse(value?.ToString() ?? string.Empty), DateTimeKind.Utc).ToLocalTime();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}