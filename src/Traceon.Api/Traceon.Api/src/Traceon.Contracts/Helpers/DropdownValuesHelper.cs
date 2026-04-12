namespace Traceon.Contracts.Helpers;

public static class DropdownValuesHelper
{
    public const char Delimiter = '|';
    private static readonly char[] DelimiterArray = [Delimiter];

    public static string[] Split(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) return [];
        return csv.Split(DelimiterArray, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public static string Join(IEnumerable<string> values) => string.Join(Delimiter, values);
    public static string Join(params string[] values) => string.Join(Delimiter, values);

    public static string Append(string? existing, string newValue)
    {
        if (string.IsNullOrWhiteSpace(existing)) return newValue;
        return existing + Delimiter + newValue;
    }
}
