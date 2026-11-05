namespace Services.Models;

public class ProductPageResult
{
    public required IReadOnlyList<Product> Items { get; set; }

    public required long TotalCount { get; set; }
}