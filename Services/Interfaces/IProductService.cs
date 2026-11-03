using Common.Models;
using Common.Responses;

namespace Services.Interfaces;

public interface IProductService
{
    public ResponseResult<IEnumerable<ProductResponse>> Get();
}