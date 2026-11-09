namespace Common.Models;

public class ProductCursor
{
    public required DateTimeOffset LastDate { get; set; }
    public required Guid LastId { get; set; }
}