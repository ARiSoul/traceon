using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Traceon.Domain.Repositories;
using Traceon.Infrastructure.Email;
using Traceon.Infrastructure.Identity;
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

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            });

        services.AddAuthorizationBuilder();

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

        return services;
    }

    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TraceonDbContext>();
        await context.Database.MigrateAsync();
    }
}
