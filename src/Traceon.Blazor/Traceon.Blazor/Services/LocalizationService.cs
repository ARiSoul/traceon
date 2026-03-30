using System.Globalization;
using Blazored.LocalStorage;
using Microsoft.JSInterop;

namespace Traceon.Blazor.Services;

public sealed class LocalizationService(ILocalStorageService localStorage, IJSRuntime js)
{
    private const string LanguageKey = "traceon_language";

    public static readonly (string Code, string Name)[] SupportedLanguages =
    [
        ("en", "English"),
        ("pt", "Português"),
        ("es", "Español")
    ];

    public string CurrentLanguage { get; private set; } = "en";

    public async Task InitializeAsync()
    {
        var stored = await localStorage.GetItemAsStringAsync(LanguageKey);

        if (!string.IsNullOrEmpty(stored) && SupportedLanguages.Any(l => l.Code == stored))
        {
            CurrentLanguage = stored;
        }

        ApplyCulture(CurrentLanguage);
    }

    public async Task SetLanguageAsync(string languageCode)
    {
        if (CurrentLanguage == languageCode) return;

        CurrentLanguage = languageCode;
        await localStorage.SetItemAsStringAsync(LanguageKey, languageCode);

        // Reload the app to apply the new culture everywhere
        await js.InvokeVoidAsync("location.reload");
    }

    internal async Task<bool> ApplyFromServer(string? serverLanguage)
    {
        if (string.IsNullOrEmpty(serverLanguage) ||
            !SupportedLanguages.Any(l => l.Code == serverLanguage) ||
            serverLanguage == CurrentLanguage)
            return false;

        CurrentLanguage = serverLanguage;
        await localStorage.SetItemAsStringAsync(LanguageKey, serverLanguage);
        ApplyCulture(serverLanguage);
        return true;
    }

    private static void ApplyCulture(string languageCode)
    {
        var culture = new CultureInfo(languageCode);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
