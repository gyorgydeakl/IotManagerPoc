using Microsoft.EntityFrameworkCore;

namespace IotManagerApi.Database;

public class IotManagerDbContext(DbContextOptions<IotManagerDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<BatchJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
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
        });
    }

    public DbSet<BatchJob> DeviceGroups { get; set; }
}