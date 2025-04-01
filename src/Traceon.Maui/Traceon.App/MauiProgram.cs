using Arisoul.Traceon.Maui.Core.Interfaces;
using Arisoul.Traceon.Maui.Infrastructure.Services;
using Arisoul.Traceon.Maui.Infrastructure.Storage;
using Microsoft.Extensions.Logging;
using Arisoul.Traceon.App.ViewModels;
using System.Globalization;
using CommunityToolkit.Maui;
using Arisoul.Core.Maui;

namespace Arisoul.Traceon.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseArisoulMaui()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("fa-brands-400.ttf", "FAB");
                fonts.AddFont("fa-regular-400.ttf", "FAR");
                fonts.AddFont("fa-solid-900.ttf", "FAS");
                fonts.AddFont("fa-v4compatibility.ttf", "FACOMP");
            });

        // TODO: replace with real userId (from auth or config)
        var userId = "demo";

        // Dependency injection
        builder.Services.AddSingleton<ITrackedActionRepository>(_ => new JsonTrackedActionRepository(userId));
        builder.Services.AddSingleton<IActionEntryRepository>(_ => new JsonActionEntryRepository(userId));
        builder.Services.AddSingleton<IAnalyticsService, BasicAnalyticsService>();

        builder.Services.AddTransient<TrackedActionsViewModel>();
        builder.Services.AddTransient<TrackedActionCreateOrEditViewModel>();
        builder.Services.AddTransient<ActionEntryCreateOrEditViewModel>();

        builder.Services.AddTransient<Views.TrackedActionsPage>();
        builder.Services.AddTransient<Views.TrackedActionCreateOrEditPage>();
        builder.Services.AddTransient<Views.ActionEntryCreateOrEditPage>();

        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
