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
}