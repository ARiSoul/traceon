using Arisoul.Core.Maui;
using Arisoul.Traceon.App.ViewModels;
using Arisoul.Traceon.Maui.Core;
using Arisoul.Traceon.Maui.Core.Interfaces;
using Arisoul.Traceon.Maui.Infrastructure.Data;
using Arisoul.Traceon.Maui.Infrastructure.Repositories;
using Arisoul.Traceon.Maui.Infrastructure.UnitOfWork;
using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using System.Globalization;

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

        // Repositories and Unit of Work
        builder.Services.AddScoped<ITrackedActionRepository, TrackedActionRepository>();
        builder.Services.AddScoped<IFieldDefinitionRepository, FieldDefinitionRepository>();
        builder.Services.AddScoped<ITagRepository, TagRepository>();
        builder.Services.AddScoped<IActionFieldRepository, ActionFieldRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        // View models
        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<TrackedActionsViewModel>();
        builder.Services.AddTransient<TrackedActionCreateOrEditViewModel>();
        builder.Services.AddTransient<ActionEntryCreateOrEditViewModel>();
        builder.Services.AddTransient<FieldDefinitionsViewModel>();
        builder.Services.AddTransient<FieldDefinitionCreateOrEditViewModel>();

        // Pages
        builder.Services.AddTransient<Views.MainPage>();
        builder.Services.AddTransient<Views.TrackedActionsPage>();
        builder.Services.AddTransient<Views.TrackedActionCreateOrEditPage>();
        builder.Services.AddTransient<Views.ActionEntryCreateOrEditPage>();
        builder.Services.AddTransient<Views.FieldDefinitionsPage>();
        builder.Services.AddTransient<Views.FieldDefinitionCreateOrEditPage>();

        // Database context
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "traceon.db");
        builder.Services.AddDbContext<TraceonDbContext>(options =>
            options.UseSqlite($"Filename={dbPath}"));

        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

        // Add Syncfusion
        builder.ConfigureSyncfusionCore();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Ensure database is created and migrations applied
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TraceonDbContext>();
            dbContext.Database.Migrate();
        }


        return app;
    }
}
