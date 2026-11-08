namespace Common.Responses;

public class ProductResponse
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required string ImgUri { get; set; }
    public required decimal Price { get; set; }
    public string? Description { get; set; }
}
