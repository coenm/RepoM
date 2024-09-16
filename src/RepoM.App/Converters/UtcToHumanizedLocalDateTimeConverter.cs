namespace RepoM.App.Converters;

using System;
using System.Globalization;
using System.Windows.Data;
using RepoM.Api.Common;

/// <summary>
/// The UtcToHumanizedLocalDateTimeConverter class is a WPF value converter that
/// converts a UTC DateTime to a human-readable local time string.
/// It implements the IValueConverter interface and uses a HardcodededMiniHumanizer to format the date.
/// The Convert method handles the conversion, while the ConvertBack method is not implemented.
/// </summary>
public class UtcToHumanizedLocalDateTimeConverter : IValueConverter
{
    private static readonly HardcodededMiniHumanizer _humanizer = new(SystemClock.Instance);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        DateTime parse;
        var ok = DateTime.TryParse(value?.ToString() ?? string.Empty, CultureInfo.InvariantCulture, out parse);
        if (!ok)
        {
            return null;
        }
        var date = DateTime.SpecifyKind(parse, DateTimeKind.Utc).ToLocalTime();
        return _humanizer.HumanizeTimestamp(date);
        
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
