using System.Net;
using System.Net.Mime;
using Api.Extensions;
using Api.Models.Requests;
using Asp.Versioning;
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
        var offset = ((long)request.PageNumber - 1) * request.PageSize;

        if (offset > int.MaxValue)
        {
            ModelState.AddModelError("pageNumber", "The requested page offset exceeds the supported limit.");
            return ValidationProblem(ModelState);
        }

        var response = await service.GetPageAsync(request.PageNumber, request.PageSize, (int)offset, cancellationToken);
        return response.ToActionResult();
    }
}