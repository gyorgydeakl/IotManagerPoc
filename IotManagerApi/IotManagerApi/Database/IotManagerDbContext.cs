using Microsoft.EntityFrameworkCore;

namespace IotManagerApi.Database;

public class IotManagerDbContext : DbContext
{
    public IotManagerDbContext(DbContextOptions<IotManagerDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<DeviceGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.OwnsMany(e => e.DeviceIds);
        });
    }

    public DbSet<DeviceGroup> DeviceGroups { get; set; }
}