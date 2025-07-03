using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IotManagerApi.Database;

public class IotManagerDbContext(DbContextOptions<IotManagerDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        var jsonNodeConverter = new ValueConverter<JsonNode,string>(
            n => n.ToJsonString(null),
            s => JsonNode.Parse(s, null,  default)!);

        modelBuilder.Entity<BatchJob>(entity =>
        {
            entity.ToTable(nameof(BatchJob));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.OwnsMany(e => e.DeviceIds,tag =>
            {
                tag.Property(t => t.Value).IsRequired();
            });
            entity.OwnsMany(e => e.TagsToSet, tag =>
            {
                tag.Property(t => t.Key).IsRequired().HasMaxLength(100);
                tag.Property(t => t.Value).IsRequired().HasMaxLength(500);
            });
            entity.OwnsMany(e => e.TagsToDelete, tag =>
            {
                tag.Property(t => t.Value).IsRequired().HasMaxLength(100);
            });
            entity.OwnsMany(e => e.PropertiesToSet, nb =>
            {
                nb.Property(p => p.Key).IsRequired();
                nb.Property(p => p.Value)
                    .IsRequired()
                    .HasColumnType("nvarchar(max)")
                    .HasConversion(jsonNodeConverter);
            });
            entity.OwnsMany(e => e.PropertiesToDelete, p =>
            {
                p.Property(d => d.Value).IsRequired();
            });
        });
    }

    public DbSet<BatchJob> DeviceGroups { get; set; }
}