using Common.Models;
using Common.Responses;

namespace Services.Models;

public class ProductPageResult
{
    public required IReadOnlyList<ProductResponse> Items { get; set; }

    public required int PageSize { get; set; }

    public required ProductCursor? NextCursor { get; set; }
}