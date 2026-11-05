using Common.Models;
using Common.Responses;
using Services.Interfaces;
using Services.Models;

namespace Services.Services;

public class ProductService(IProductRepository productRepository): IProductService
{
    public async Task<ResponseResult<IEnumerable<ProductResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var response = (await productRepository.GetAllAsync(cancellationToken)).ToResponses();
        return ResponseResult<IEnumerable<ProductResponse>>.Success(response);
    }

    public async Task<ResponseResult<ProductResponse>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var data = await productRepository.GetAsync(id, cancellationToken);
        if (data == null)
            return ResponseResult<ProductResponse>.NotFound();
        
        var response = data.ToResponse();
        return ResponseResult<ProductResponse>.Success(response);
    }
}