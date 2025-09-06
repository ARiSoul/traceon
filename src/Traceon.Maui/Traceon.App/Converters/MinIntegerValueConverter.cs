using System.Globalization;

namespace Arisoul.Traceon.App.Converters;

public class MinIntegerValueConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return int.MinValue;

        return value;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return int.MinValue;

        return value;
    }
}
