using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Traceon.Domain.Repositories;
using Traceon.Infrastructure.Identity;
using Traceon.Infrastructure.Persistence;
using Traceon.Infrastructure.Persistence.Repositories;

namespace Traceon.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<TraceonDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("MigrationsHistory", TraceonDbContext.Schema)));

        services.AddIdentityApiEndpoints<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<TraceonDbContext>();

        services.AddAuthorizationBuilder();

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
