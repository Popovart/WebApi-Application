using System.Net;
using System.Net.Mime;
using Api.Extensions;
using Api.Models.Requests;
using Common.Responses;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Api.Controllers;

[ApiController]
[Route("products")]
public class ProductsController(IProductService service) : ControllerBase
{
    [HttpGet(Name = "Products_GetAll")]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), (int)HttpStatusCode.OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var response = await service.GetAllAsync(cancellationToken);
        return response.ToActionResult();
    }

    [HttpGet("{id}", Name = "Products_GetById")]
    [ProducesResponseType(typeof(ProductResponse), (int)HttpStatusCode.OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest, MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult<ProductResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await service.GetAsync(id, cancellationToken);
        return response.ToActionResult();
    }

    [HttpPatch("{id}/description", Name = "Products_PatchDescription")]
    [ProducesResponseType(typeof(ProductResponse), (int)HttpStatusCode.OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.UnsupportedMediaType, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult<ProductResponse>> PatchDescription(
        [FromRoute] Guid id,
        [FromBody] PatchProductDescriptionRequest request,
        CancellationToken cancellationToken)
    {
        var response = await service.PatchDescriptionAsync(id, request.Description, cancellationToken);
        return response.ToActionResult();
    }
}
