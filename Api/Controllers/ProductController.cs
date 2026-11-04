using System.Net;
using System.Net.Mime;
using Api.Extensions;
using Common.Responses;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController(IProductService service) : ControllerBase
{
    [HttpGet(Name = "getAll")]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), (int)HttpStatusCode.OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public ActionResult<IEnumerable<ProductResponse>> GetAll()
    {
        var response = service.GetAll();
        return response.ToActionResult();
    }
}
