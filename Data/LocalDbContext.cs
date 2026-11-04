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
            e.Property(x => x.Price).HasPrecision(18, 2);
        });
    }
}
 