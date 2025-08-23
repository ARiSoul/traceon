// Ignore Spelling: Traceon

using Arisoul.Core.Root.Extensions;
using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Maui.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Arisoul.Traceon.Maui.Infrastructure.Data;

public class TraceonDbContext(DbContextOptions<TraceonDbContext> options) : DbContext(options)
{
    public DbSet<TrackedAction> TrackedActions => Set<TrackedAction>();
    public DbSet<ActionEntry> ActionEntries => Set<ActionEntry>();
    public DbSet<ActionTag> ActionTags => Set<ActionTag>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ActionField> ActionFields => Set<ActionField>();
    public DbSet<FieldDefinition> FieldDefinitions => Set<FieldDefinition>();
    public DbSet<Audit> Audits => Set<Audit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // JSON conversion for FieldValues
        modelBuilder.Entity<ActionEntry>()
            .Property(e => e.FieldValues)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null))
            .HasColumnType("TEXT");

        // Composite key for ActionTag (many-to-many)
        modelBuilder.Entity<ActionTag>()
            .HasKey(at => new { at.ActionId, at.TagId });

        modelBuilder.Entity<ActionTag>()
            .HasOne(at => at.Action)
            .WithMany(a => a.Tags)
            .HasForeignKey(at => at.ActionId);

        modelBuilder.Entity<ActionTag>()
            .HasOne(at => at.Tag)
            .WithMany()
            .HasForeignKey(at => at.TagId);

        // Composite key for ActionField (linking to FieldDefinition)
        modelBuilder.Entity<ActionField>()
            .HasKey(af => new { af.ActionId, af.FieldDefinitionId });

        modelBuilder.Entity<ActionField>()
            .HasOne(af => af.Action)
            .WithMany(a => a.Fields)
            .HasForeignKey(af => af.ActionId);

        modelBuilder.Entity<ActionField>()
            .HasOne(af => af.FieldDefinition)
            .WithMany()
            .HasForeignKey(af => af.FieldDefinitionId);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = await base.SaveChangesAsync(cancellationToken);

        if (auditEntries.Count != 0)
        {
            await Audits.AddRangeAsync(auditEntries, cancellationToken);
            await base.SaveChangesAsync(cancellationToken);
        }
        return result;
    }

    private List<Audit> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<Audit>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is Audit || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var audit = new Audit
            {
                Entity = entry.Entity.GetType().Name,
                Operation = entry.State.ToString().ToLowerInvariant(),
                Timestamp = DateTime.UtcNow,
                User = "system" // Replace with your user context when available
            };

            if (entry.Entity is IEntityWithId entityWithId)
            {
                audit.EntityId = entityWithId.Id.ToString();
            }
            else if (entry.Entity is IActionChildEntity actionChildEntity)
            {
                audit.EntityId = actionChildEntity.ActionId.ToString();

                if (entry.Entity is ActionField actionField)
                {
                    audit.EntityId = $"{audit.EntityId}-{actionField.FieldDefinitionId}";
                }
                else if (entry.Entity is ActionTag actionTag)
                {
                    audit.EntityId = $"{audit.EntityId}-{actionTag.TagId}";
                }
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    audit.NewValues = SerializeValues(entry.CurrentValues);
                    break;

                case EntityState.Deleted:
                    audit.OldValues = SerializeValues(entry.OriginalValues);
                    break;

                case EntityState.Modified:
                    audit.OldValues = SerializeValues(entry.OriginalValues);
                    audit.NewValues = SerializeValues(entry.CurrentValues);
                    break;
            }

            if (audit.OldValues != audit.NewValues)
                auditEntries.Add(audit);
        }

        return auditEntries;
    }

    private static string SerializeValues(PropertyValues values)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var property in values.Properties)
        {
            dict[property.Name] = values[property];
        }

        return JsonSerializer.Serialize(dict);
    }


}
