using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Traceon.Blazor;
using Traceon.Blazor.Auth;
using Traceon.Blazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? "http://localhost:5285";

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<TokenStore>();
builder.Services.AddScoped<TokenAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<TokenAuthStateProvider>());
builder.Services.AddAuthorizationCore();

builder.Services.AddTransient<AuthTokenHandler>();

builder.Services.AddHttpClient("TraceonApiAnonymous", client =>
    client.BaseAddress = new Uri(apiBaseAddress));

builder.Services.AddHttpClient("TraceonApi", client =>
    client.BaseAddress = new Uri(apiBaseAddress))
    .AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("TraceonApi"));

builder.Services.AddLocalization();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TrackedActionService>();
builder.Services.AddScoped<TagService>();
builder.Services.AddScoped<FieldDefinitionService>();
builder.Services.AddScoped<DropdownValueService>();
builder.Services.AddScoped<DropdownValueMetadataService>();
builder.Services.AddScoped<ActionFieldService>();
builder.Services.AddScoped<ActionEntryService>();
builder.Services.AddScoped<EntryTemplateService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<FieldAnalyticsRuleService>();
builder.Services.AddScoped<FieldDependencyRuleService>();
builder.Services.AddScoped<ConnectedActionRuleService>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<DataPortabilityService>();
builder.Services.AddScoped<ReceiptScanService>();
builder.Services.AddScoped<ReceiptScanDraftService>();
builder.Services.AddScoped<ReceiptImportConfigService>();
builder.Services.AddScoped<ChartVisibilityService>();
builder.Services.AddScoped<CustomChartApiService>();
builder.Services.AddScoped<FeedbackService>();
builder.Services.AddScoped<TemplateService>();
builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<LocalizationService>();
builder.Services.AddScoped<UserPreferenceService>();
builder.Services.AddScoped<TrashService>();
builder.Services.AddScoped<DropdownCoordinator>();

var host = builder.Build();

var localizationService = host.Services.GetRequiredService<LocalizationService>();
await localizationService.InitializeAsync();

var themeService = host.Services.GetRequiredService<ThemeService>();
await themeService.InitializeAsync();

await host.RunAsync();
