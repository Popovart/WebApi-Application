using Common.Models;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using Services.Models;

namespace Data.Repositories;

public class SqlProductRepository(LocalDbContext context): IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Set<Product>().AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetPageAsync(ProductCursor? cursor, int take, CancellationToken cancellationToken)
    {
        var query = context.Set<Product>().AsNoTracking();

        if (cursor != null)
        {
            query = query.Where(product =>
                product.CreatedAt > cursor.LastDate ||
                (product.CreatedAt == cursor.LastDate && product.Id > cursor.LastId));
        }

        return await query
            .OrderBy(product => product.CreatedAt)
            .ThenBy(product => product.Id)
            .Take(take)
            .ToListAsync(cancellationToken);
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