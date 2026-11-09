using System.Net;
using System.Net.Mime;
using Api.Extensions;
using Api.Models.Requests;
using Api.Pagination;
using Asp.Versioning;
using Common.Models;
using Common.Responses;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Api.Controllers;

[ApiController]
[Tags("Products")]
[ApiVersion("2.0")]
[Route("v{version:apiVersion}/products")]
public class ProductsV2Controller(IProductService service) : ControllerBase
{
    [HttpGet(Name = "ProductsV2_GetPage")]
    [ProducesResponseType(typeof(ProductsPageResponse), (int)HttpStatusCode.OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult<ProductsPageResponse>> GetPage([FromQuery] ProductPaginationRequest request, CancellationToken cancellationToken)
    {
        ProductCursor? cursor = null;
        if (!string.IsNullOrEmpty(request.Cursor) && !ProductCursorCodec.TryDecode(request.Cursor, out cursor))
        {
            ModelState.AddModelError("cursor", "The cursor is invalid.");
            return ValidationProblem(ModelState);
        }

        var response = await service.GetPageAsync(cursor, request.PageSize, cancellationToken);
        if (!response.IsSuccess || response.Data is not { } page)
            return response.ToActionResult();

        return new ProductsPageResponse
        {
            Items = page.Items,
            NextCursor = page.NextCursor is null ? null : ProductCursorCodec.Encode(page.NextCursor),
            PageSize = page.PageSize
        };
    }
}