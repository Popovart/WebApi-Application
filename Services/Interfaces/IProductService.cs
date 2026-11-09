using Common.Models;
using Common.Responses;
using Services.Models;

namespace Services.Interfaces;

public interface IProductService
{
    public Task<ResponseResult<IEnumerable<ProductResponse>>> GetAllAsync(CancellationToken cancellationToken);
    public Task<ResponseResult<ProductPageResult>> GetPageAsync(ProductCursor? cursor, int pageSize, CancellationToken cancellationToken);
    public Task<ResponseResult<ProductResponse>> GetAsync(Guid id, CancellationToken cancellationToken);
    public Task<ResponseResult<ProductResponse>> PatchDescriptionAsync(Guid id, string? description, CancellationToken cancellationToken);
}