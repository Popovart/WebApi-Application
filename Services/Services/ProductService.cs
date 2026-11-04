using Common.Models;
using Common.Responses;
using Services.Interfaces;
using Services.Models;

namespace Services.Services;

public class ProductService(IProductRepository productRepository): IProductService
{
    public ResponseResult<IEnumerable<ProductResponse>> GetAll()
    {
        var data = productRepository.GetAll().ToResponses();
        return ResponseResult<IEnumerable<ProductResponse>>.Success(data);
    }
}