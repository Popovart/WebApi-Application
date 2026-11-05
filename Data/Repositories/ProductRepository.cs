using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using Services.Models;

namespace Data.Repositories;

public class ProductRepository(LocalDbContext context): IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Set<Product>().AsNoTracking().ToListAsync(cancellationToken);
    }

    public Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Set<Product>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Product?> PatchDescriptionAsync(Guid id, string? description, CancellationToken cancellationToken)
    {
        var product = await context.FindAsync<Product>([id], cancellationToken: cancellationToken);
        if (product == null)
            return null;
        product.Description = description;
        await context.SaveChangesAsync(cancellationToken);
        return product;
    }
}