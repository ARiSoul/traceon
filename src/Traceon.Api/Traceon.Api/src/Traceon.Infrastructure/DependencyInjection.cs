using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Traceon.Domain.Repositories;
using Traceon.Infrastructure.Audit;
using Traceon.Infrastructure.DataPortability;
using Traceon.Infrastructure.Email;
using Traceon.Infrastructure.Identity;
using Traceon.Infrastructure.Onboarding;
using Traceon.Infrastructure.Persistence;
using Traceon.Infrastructure.Persistence.Repositories;

namespace Traceon.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TraceonDb")
            ?? throw new InvalidOperationException("Connection string 'TraceonDb' is not configured.");

        services.AddDbContext<TraceonDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("MigrationsHistory", TraceonDbContext.Schema)));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddSignInManager()
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<TraceonDbContext>();

        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings are not configured.");

        services.AddSingleton(jwtSettings);
        services.AddScoped<JwtTokenService>();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddCookie(IdentityConstants.ExternalScheme, options =>
            {
                options.Cookie.Name = ".Traceon.External";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Key))
                };
            })
            .AddExternalProviders(configuration);

        services.AddAuthorizationBuilder();

        var externalAuth = configuration.GetSection("ExternalAuth").Get<ExternalAuthSettings>() ?? new ExternalAuthSettings();
        services.AddSingleton(externalAuth);

        var emailSettings = configuration.GetSection("Email").Get<EmailSettings>() ?? new EmailSettings();
        services.AddSingleton(emailSettings);

        if (!string.IsNullOrEmpty(emailSettings.SmtpHost) && emailSettings.SmtpHost != "localhost")
            services.AddTransient<IEmailSender<ApplicationUser>, SmtpEmailSender>();
        else
            services.AddTransient<IEmailSender<ApplicationUser>, LoggingEmailSender>();

        services.AddScoped<ITrackedActionRepository, TrackedActionRepository>();
        services.AddScoped<IFieldDefinitionRepository, FieldDefinitionRepository>();
        services.AddScoped<IActionFieldRepository, ActionFieldRepository>();
        services.AddScoped<IActionEntryRepository, ActionEntryRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IFieldAnalyticsRuleRepository, FieldAnalyticsRuleRepository>();
        services.AddScoped<IFieldDependencyRuleRepository, FieldDependencyRuleRepository>();
        services.AddScoped<IReceiptImportConfigRepository, ReceiptImportConfigRepository>();
        services.AddScoped<AuditService>();
        services.AddScoped<DataPortabilityService>();
        services.AddScoped<TemplateInstallService>();
        services.AddScoped<TrashService>();

        services.Configure<PurgeSettings>(configuration.GetSection("Purge"));
        services.AddHostedService<PurgeDeletedDataService>();

        return services;
    }

    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TraceonDbContext>();
        await context.Database.MigrateAsync();
    }

    private static AuthenticationBuilder AddExternalProviders(this AuthenticationBuilder builder, IConfiguration configuration)
    {
        var settings = configuration.GetSection("ExternalAuth").Get<ExternalAuthSettings>();

        if (!string.IsNullOrEmpty(settings?.Google?.ClientId))
        {
            builder.AddGoogle(options =>
            {
                options.ClientId = settings.Google.ClientId;
                options.ClientSecret = settings.Google.ClientSecret;
                options.CallbackPath = "/api/identity/signin-google";
            });
        }

        if (!string.IsNullOrEmpty(settings?.Microsoft?.ClientId))
        {
            builder.AddMicrosoftAccount(options =>
            {
                options.ClientId = settings.Microsoft.ClientId;
                options.ClientSecret = settings.Microsoft.ClientSecret;
                options.CallbackPath = "/api/identity/signin-microsoft";
            });
        }

        return builder;
    }
}
