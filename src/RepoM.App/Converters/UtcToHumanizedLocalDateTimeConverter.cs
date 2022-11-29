namespace RepoM.App.Converters;

using System;
using System.Globalization;
using System.Windows.Data;
using RepoM.Api.Common;

public class UtcToHumanizedLocalDateTimeConverter : IValueConverter
{
    private readonly IHumanizer _humanizer;

    public UtcToHumanizedLocalDateTimeConverter()
    {
        _humanizer = new HardcodededMiniHumanizer(SystemClock.Instance);
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        DateTime date = DateTime.SpecifyKind(DateTime.Parse(value.ToString() ?? string.Empty), DateTimeKind.Utc).ToLocalTime();
        return _humanizer.HumanizeTimestamp(date);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}