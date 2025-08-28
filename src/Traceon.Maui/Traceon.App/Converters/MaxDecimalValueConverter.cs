using System.Globalization;

namespace Arisoul.Traceon.App.Converters;

public class MaxDecimalValueConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return decimal.MaxValue;

        return value;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return decimal.MaxValue;

        return value;
    }
}
