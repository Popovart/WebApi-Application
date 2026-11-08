using System.Net;
using System.Net.Mime;
using Api.Extensions;
using Api.Models.Requests;
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
        KeySet? keySet;
        switch (request)
        {
            case { LastDate: null, LastId: not null }:
                ModelState.AddModelError("LastDate", "Unable to compose keySet for the request. Given LastDate is null when LastId is not null.");
                return ValidationProblem(ModelState);
            case { LastDate: not null, LastId: null }:
                ModelState.AddModelError("LastId", "Unable to compose keySet for the request. Given LastId is null when LastDate is not null.");
                return ValidationProblem(ModelState);
            case {
                    LastDate: { } lastDate,
                    LastId: { } lastId
                 }:
                keySet = new KeySet
                {
                    LastDate = lastDate,
                    LastId = lastId
                };
                break;
            default:
                keySet = null;
                break;
        }
        
        var response = await service.GetPageAsync(keySet, request.PageSize, cancellationToken);
        return response.ToActionResult();
    }
}