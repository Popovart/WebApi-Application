using Common.Models;
using Common.Responses;
using Services.Interfaces;
using Services.Models;

namespace Services.Services;

public class ProductService: IProductService
{
    public ResponseResult<IEnumerable<ProductResponse>> Get()
    {
        List<Product> data = [
            new()
            {
                Id = Guid.NewGuid(),
                Name = "product 1",
                ImgUri = "string",
                Price = 200,
                Description = "lorum ipsum"
            }
        ];

        var responses = data.ToResponses();
        return ResponseResult<IEnumerable<ProductResponse>>.Success(responses);
    }
}