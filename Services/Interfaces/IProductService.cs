using Common.Models;
using Common.Responses;

namespace Services.Interfaces;

public interface IProductService
{
    public Task<ResponseResult<IEnumerable<ProductResponse>>> GetAllAsync(CancellationToken cancellationToken);
    public Task<ResponseResult<ProductResponse>> GetAsync(Guid id, CancellationToken cancellationToken);
}