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

    public Task<ProductPageResult> GetPageAsync(int offset, int pageSize, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult(new ProductPageResult
            {
                Items = _products
                    .OrderBy(product => product.Id)
                    .Skip(offset)
                    .Take(pageSize)
                    .Select(Copy)
                    .ToArray(),
                TotalCount = _products.Count
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
            Name = product.Name,
            ImgUri = product.ImgUri,
            Price = product.Price,
            Description = product.Description
        };
    }
}