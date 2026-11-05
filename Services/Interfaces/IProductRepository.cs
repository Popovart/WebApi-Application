using Services.Models;

namespace Services.Interfaces;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken);
    Task<Product?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<Product?> PatchDescriptionAsync(Guid id, string? description, CancellationToken cancellationToken);
}