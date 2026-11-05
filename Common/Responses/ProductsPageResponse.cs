namespace Common.Responses;

public class ProductsPageResponse
{
    public required IReadOnlyList<ProductResponse> Items { get; set; }

    public required int PageNumber { get; set; }

    public required int PageSize { get; set; }

    public required long TotalCount { get; set; }
}