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

    public async Task<ResponseResult<ProductsPageResponse>> GetPageAsync(KeySet? keySet, int pageSize, CancellationToken cancellationToken)
    {
        var page = await productRepository.GetPageAsync(keySet, pageSize, cancellationToken);

        var response = new ProductsPageResponse
        {
            Items = page.Items.ToResponses().ToArray(),
            KeySet = page.KeySet,
            PageSize = pageSize,
            TotalCount = page.TotalCount
        };

        return ResponseResult<ProductsPageResponse>.Success(response);
    }

    public async Task<ResponseResult<ProductResponse>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var data = await productRepository.GetAsync(id, cancellationToken);
        if (data == null)
            return ResponseResult<ProductResponse>.NotFound();
        
        var response = data.ToResponse();
        return ResponseResult<ProductResponse>.Success(response);
    }

    public async Task<ResponseResult<ProductResponse>> PatchDescriptionAsync(Guid id, string? description, CancellationToken cancellationToken)
    {
        var patchResult = await productRepository.PatchDescriptionAsync(id, description, cancellationToken);
        if (patchResult == null)
            return ResponseResult<ProductResponse>.NotFound();

        var response = patchResult.ToResponse();
        return ResponseResult<ProductResponse>.Success(response);
    }
}