using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using Services.Models;

namespace Data.Repositories;

public class ProductRepository(LocalDbContext context): IProductRepository
{
    public IReadOnlyList<Product> GetAll()
    {
        return context.Set<Product>().AsNoTracking().ToList();
    }
}