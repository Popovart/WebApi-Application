
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Api.Models.Requests;

public class ProductPaginationRequest
{
    [DefaultValue(1)]
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [DefaultValue(10)]
    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}