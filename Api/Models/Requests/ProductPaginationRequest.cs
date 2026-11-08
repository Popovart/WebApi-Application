
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Api.Models.Requests;

public class ProductPaginationRequest
{
    public required DateTimeOffset? LastDate { get; set; }
    
    public required Guid? LastId { get; set; }

    [DefaultValue(10)]
    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}