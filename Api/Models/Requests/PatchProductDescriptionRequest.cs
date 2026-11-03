namespace Api.models.Requests;

public class PatchProductDescriptionRequest
{
    public required Guid Id { get; set; }
    public required string? Description { get; set; }
}
