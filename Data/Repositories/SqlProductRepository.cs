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

    public async Task<ProductPageResult> GetPageAsync(KeySet? keySet, int pageSize, CancellationToken cancellationToken)
    {
        var query = context.Set<Product>().AsNoTracking();

        var totalCount = await query.LongCountAsync(cancellationToken);

        query = query
            .OrderBy(product => product.CreatedAt)
            .ThenBy(product => product.Id);
        
        if (keySet != null)
        {
            query = query.Where(
                x => x.CreatedAt > keySet.LastDate || 
                (x.CreatedAt == keySet.LastDate && x.Id > keySet.LastId));
        }    
        var items = await query.Take(pageSize).ToListAsync(cancellationToken);    
        
        var last = items.LastOrDefault();

        KeySet? keySetResult;
        if (last == null)
        {
            if (keySet == null)
                keySetResult = null;
            else
                keySetResult = new KeySet
                {
                    LastDate = keySet.LastDate,
                    LastId = keySet.LastId
                };
        }
        else
        {
            keySetResult = new KeySet
            {
                LastDate = last.CreatedAt,
                LastId = last.Id
            };
        }
        
        return new ProductPageResult
        {
            Items = items,
            TotalCount = totalCount,
            KeySet = keySetResult
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