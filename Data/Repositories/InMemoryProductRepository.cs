using Common.Models;
using Data.Models;
using Services.Interfaces;
using Services.Models;

namespace Data.Repositories;

public sealed class InMemoryProductRepository : IProductRepository
{
    private readonly Lock _sync = new(); 
    private readonly List<Product> _products = MockProductData.Create();

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult<IReadOnlyList<Product>>(_products.Select(Copy).ToArray());
        }
    }

    public Task<ProductPageResult> GetPageAsync(KeySet? keySet, int pageSize, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            IEnumerable<Product> query = _products
                .OrderBy(product => product.CreatedAt)
                .ThenBy(product => product.Id);
            
            if (keySet != null)
            {
                query = query.Where(x =>
                    x.CreatedAt > keySet.LastDate || (x.CreatedAt == keySet.LastDate && x.Id > keySet.LastId));
            }    
            
            var items = query.Take(pageSize).Select(Copy).ToArray();
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
            return Task.FromResult(new ProductPageResult
            {
                Items = items,
                TotalCount = _products.Count,
                KeySet = keySetResult
            });
        }
    }

    public Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            var product = FindStoredProduct(id);

            return Task.FromResult(product is null ? null : Copy(product));
        }
    }

    public Task<Product?> PatchDescriptionAsync(Guid id, string? description, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            var product = FindStoredProduct(id);

            if (product is null)
                return Task.FromResult<Product?>(null);

            product.Description = description;

            return Task.FromResult<Product?>(Copy(product));
        }
    }

    private Product? FindStoredProduct(Guid id)
    {
        return _products.FirstOrDefault(product => product.Id == id);
    }

    private static Product Copy(Product product)
    {
        return new Product
        {
            Id = product.Id,
            CreatedAt = product.CreatedAt,
            Name = product.Name,
            ImgUri = product.ImgUri,
            Price = product.Price,
            Description = product.Description
        };
    }
}