namespace Traceon.Blazor.Components;

/// <summary>
/// Represents an action in a list dropdown menu.
/// </summary>
public class ListAction
{
    public string Text { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? Href { get; set; }
    public Func<Task>? OnClick { get; set; }
    public ListActionStyle Style { get; set; } = ListActionStyle.Default;
    public bool IsSeparator { get; set; }
    public bool IsDisabled { get; set; }
    public string? Tooltip { get; set; }

    public static ListAction Separator() => new() { IsSeparator = true };

    public static ListAction Disabled(string text, string icon = "", string? tooltip = null) => new()
    {
        Text = text,
        Icon = icon,
        IsDisabled = true,
        Tooltip = tooltip ?? "This action is not available",
        Style = ListActionStyle.Secondary
    };

    public static ListAction Link(string text, string href, string icon = "", ListActionStyle style = ListActionStyle.Default) => new()
    {
        Text = text,
        Href = href,
        Icon = icon,
        Style = style
    };

    public static ListAction Button(string text, Func<Task> onClick, string icon = "", ListActionStyle style = ListActionStyle.Default) => new()
    {
        Text = text,
        OnClick = onClick,
        Icon = icon,
        Style = style
    };

    public static ListAction Button(string text, Action onClick, string icon = "", ListActionStyle style = ListActionStyle.Default) => new()
    {
        Text = text,
        OnClick = () => { onClick(); return Task.CompletedTask; },
        Icon = icon,
        Style = style
    };
}

public enum ListActionStyle
{
    Default,
    Primary,
    Success,
    Warning,
    Danger,
    Info,
    Secondary
}
