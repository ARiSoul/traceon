using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Traceon.Domain.Entities;

namespace Traceon.Infrastructure.Persistence.Configurations;

internal sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.Color)
            .IsRequired()
            .HasMaxLength(7);

        builder.HasIndex(e => new { e.UserId, e.Name })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
