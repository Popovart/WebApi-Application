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

    public async Task<ResponseResult<ProductPageResult>> GetPageAsync(ProductCursor? cursor, int pageSize, CancellationToken cancellationToken)
    {
        var products = await productRepository.GetPageAsync(cursor, pageSize + 1, cancellationToken);
        var returnedCount = Math.Min(products.Count, pageSize);
        var items = products.Take(returnedCount).ToResponses().ToArray();

        ProductCursor? nextCursor = null;
        if (products.Count > pageSize)
        {
            var last = products[returnedCount - 1];
            nextCursor = new ProductCursor { LastDate = last.CreatedAt, LastId = last.Id };
        }

        return ResponseResult<ProductPageResult>.Success(new ProductPageResult
        {
            Items = items,
            PageSize = pageSize,
            NextCursor = nextCursor
        });
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