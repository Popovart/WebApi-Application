using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Api.Models.Requests;

public class ProductPaginationRequest
{
    [FromQuery(Name = "cursor")]
    [DisplayFormat(ConvertEmptyStringToNull = false)]
    public string? Cursor { get; set; }

    [FromQuery(Name = "pageSize")]
    [DefaultValue(10)]
    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}