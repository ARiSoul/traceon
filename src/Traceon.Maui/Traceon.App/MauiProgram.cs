using Arisoul.Traceon.Maui.Core.Interfaces;
using Arisoul.Traceon.Maui.Infrastructure.Services;
using Arisoul.Traceon.Maui.Infrastructure.Storage;
using Microsoft.Extensions.Logging;
using Arisoul.Traceon.App.ViewModels;

namespace Arisoul.Traceon.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

        // TODO: replace with real userId (from auth or config)
        var userId = "demo";

        // Dependency injection
        builder.Services.AddSingleton<ITrackedActionRepository>(_ => new JsonTrackedActionRepository(userId));
        builder.Services.AddSingleton<IActionEntryRepository>(_ => new JsonActionEntryRepository(userId));
        builder.Services.AddSingleton<IAnalyticsService, BasicAnalyticsService>();

        builder.Services.AddSingleton<TrackedActionsViewModel>();

        builder.Services.AddTransient<Views.TrackedActionsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
