using Microsoft.EntityFrameworkCore;
using Services.Models;

namespace Data;

public class LocalDbContext(DbContextOptions<LocalDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("Products", "dbo");
            e.Property(x => x.Id).ValueGeneratedNever();
            e.HasIndex(x => x.CreatedAt);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')");
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.Property(x => x.Description).HasMaxLength(2000);
        });
    }
}
 