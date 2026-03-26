using Blazored.LocalStorage;
using Microsoft.JSInterop;

namespace Traceon.Blazor.Services;

public sealed class ThemeService(ILocalStorageService localStorage, IJSRuntime js)
{
    private const string ThemeKey = "traceon_theme";

    public string CurrentTheme { get; private set; } = "system";

    public async Task InitializeAsync()
    {
        var stored = await localStorage.GetItemAsStringAsync(ThemeKey);
        CurrentTheme = string.IsNullOrEmpty(stored) ? "system" : stored;
        await ApplyThemeAsync();
    }

    public async Task SetThemeAsync(string theme)
    {
        CurrentTheme = theme;
        await localStorage.SetItemAsStringAsync(ThemeKey, theme);
        await ApplyThemeAsync();
    }

    private async Task ApplyThemeAsync()
    {
        var resolved = CurrentTheme == "system" ? "" : CurrentTheme;
        await js.InvokeVoidAsync("applyTheme", resolved);
    }
}
