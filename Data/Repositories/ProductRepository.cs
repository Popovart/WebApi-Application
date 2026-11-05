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

    public async Task<ProductPageResult> GetPageAsync(int offset, int pageSize, CancellationToken cancellationToken)
    {
        var query = context.Set<Product>().AsNoTracking();

        var totalCount = await query.LongCountAsync(cancellationToken);

        var items = await query
            .OrderBy(product => product.Id)
            .Skip(offset)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new ProductPageResult
        {
            Items = items,
            TotalCount = totalCount
        };
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