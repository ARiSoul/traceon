namespace Traceon.Blazor.Helpers;

/// <summary>
/// Centralises the delimiter used to store multiple dropdown values in a single string.
/// The pipe character (<c>|</c>) is used instead of comma so that values like
/// "Pingo Doce - Distribuição Alimentar, S.A." are safely supported.
/// </summary>
public static class DropdownValuesHelper
{
    /// <summary>Storage delimiter — never exposed to end-users.</summary>
    public const char Delimiter = '|';

    private static readonly char[] DelimiterArray = [Delimiter];

    /// <summary>Splits a stored dropdown-values string into individual values.</summary>
    public static string[] Split(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
            return [];

        return csv.Split(DelimiterArray, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    /// <summary>
    /// Splits user-typed text into individual values.
    /// Accepts both comma and pipe as separators so that existing stored values
    /// and fresh user input are handled uniformly.
    /// </summary>
    public static string[] SplitUserInput(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return [];

        return input.Split([',', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    /// <summary>Joins values into the storage format.</summary>
    public static string Join(IEnumerable<string> values)
        => string.Join(Delimiter, values);

    /// <summary>Joins values into the storage format.</summary>
    public static string Join(params string[] values)
        => string.Join(Delimiter, values);

    /// <summary>Appends a single value to an existing stored string (returns the new stored string).</summary>
    public static string Append(string? existing, string newValue)
    {
        if (string.IsNullOrWhiteSpace(existing))
            return newValue;

        return $"{existing}{Delimiter}{newValue}";
    }

    /// <summary>
    /// Formats values for display to the user (comma-separated, human-readable).
    /// This is for read-only UI only — never parsed back.
    /// </summary>
    public static string FormatForDisplay(string? stored, int maxItems = int.MaxValue)
    {
        if (string.IsNullOrWhiteSpace(stored))
            return "";

        var values = Split(stored);
        var subset = maxItems < values.Length ? values.Take(maxItems) : values;
        return string.Join(", ", subset);
    }
}
