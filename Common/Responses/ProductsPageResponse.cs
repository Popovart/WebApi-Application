namespace Common.Responses;

public class ProductsPageResponse
{
    public required IReadOnlyList<ProductResponse> Items { get; set; }

    public required string? NextCursor { get; set; }

    public required int PageSize { get; set; }
}