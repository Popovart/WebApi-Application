using System.ComponentModel.DataAnnotations;

namespace Api.Models.Requests;

public class PatchProductDescriptionRequest
{
    [MaxLength(2000)]
    public required string? Description { get; set; }
}
