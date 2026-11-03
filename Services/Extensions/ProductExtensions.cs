using Common.Responses;

namespace Services.Models;

public static class ProductExtensions
{
    public static ProductResponse ToResponse(this Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            ImgUri = product.ImgUri,
            Price = product.Price,
            Description = product.Description,
        };
    }

    public static IEnumerable<ProductResponse> ToResponses(this IEnumerable<Product> products) => 
        products.Select(ToResponse);
}