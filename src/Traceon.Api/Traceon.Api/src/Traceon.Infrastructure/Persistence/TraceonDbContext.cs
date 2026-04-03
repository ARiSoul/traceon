using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Traceon.Domain.Entities;
using Traceon.Infrastructure.Audit;
using Traceon.Infrastructure.Identity;

namespace Traceon.Infrastructure.Persistence;

public sealed class TraceonDbContext(DbContextOptions<TraceonDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<TrackedAction> TrackedActions => Set<TrackedAction>();
    public DbSet<FieldDefinition> FieldDefinitions => Set<FieldDefinition>();
    public DbSet<ActionField> ActionFields => Set<ActionField>();
    public DbSet<ActionEntry> ActionEntries => Set<ActionEntry>();
    public DbSet<ActionEntryField> ActionEntryFields => Set<ActionEntryField>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TrackedActionTag> TrackedActionTags => Set<TrackedActionTag>();
    public DbSet<FieldAnalyticsRule> FieldAnalyticsRules => Set<FieldAnalyticsRule>();
    public DbSet<UserAuditLog> UserAuditLogs => Set<UserAuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TraceonDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(Entity).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.Property(parameter, nameof(Entity.IsDeleted));
            var filter = Expression.Lambda(Expression.Not(property), parameter);
            entityType.SetQueryFilter(filter);
        }
    }

#pragma warning disable IDE1006 // Naming Styles
    internal const string Schema = "Traceon";
#pragma warning restore IDE1006 // Naming Styles
}
