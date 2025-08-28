using System.Globalization;

namespace Arisoul.Traceon.App.Converters;

public class MinDecimalValueConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return decimal.MinValue;

        return value;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return decimal.MinValue;

        return value;
    }
}
