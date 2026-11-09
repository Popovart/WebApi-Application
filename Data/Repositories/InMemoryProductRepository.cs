using System.Data.SqlTypes;
using Common.Models;
using Data.Models;
using Services.Interfaces;
using Services.Models;

namespace Data.Repositories;

public sealed class InMemoryProductRepository : IProductRepository
{
    private readonly Lock _sync = new(); 
    private readonly List<Product> _products;

    public InMemoryProductRepository()
    {
        _products = MockProductData.Create();
    }

    internal InMemoryProductRepository(IEnumerable<Product> products)
    {
        _products = products.Select(Copy).ToList();
    }

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult<IReadOnlyList<Product>>(_products.Select(Copy).ToArray());
        }
    }

    public Task<IReadOnlyList<Product>> GetPageAsync(ProductCursor? cursor, int take, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            IEnumerable<Product> query = _products;
            if (cursor != null)
            {
                var lastId = new SqlGuid(cursor.LastId);
                query = query.Where(product =>
                    product.CreatedAt > cursor.LastDate ||
                    (product.CreatedAt == cursor.LastDate && new SqlGuid(product.Id).CompareTo(lastId) > 0));
            }

            var items = query
                .OrderBy(product => product.CreatedAt)
                .ThenBy(product => new SqlGuid(product.Id))
                .Take(take)
                .Select(Copy)
                .ToArray();

            return Task.FromResult<IReadOnlyList<Product>>(items);
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