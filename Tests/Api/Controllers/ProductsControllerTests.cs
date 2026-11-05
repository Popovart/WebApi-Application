using Api.Controllers;
using Api.Models.Requests;
using Common.Responses;
using Data.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Services.Services;

namespace Tests.Api.Controllers;

public class ProductsControllerTests
{
    [Fact]
    public async Task GetById_MissingProduct_Returns404Problem()
    {
        var service = new ProductService(new InMemoryProductRepository());
        var controller = new ProductsController(service);
        var missingId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");

        var result = await controller.GetById(missingId, CancellationToken.None);

        var response = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(404, response.StatusCode);

        var problem = Assert.IsAssignableFrom<ProblemDetails>(response.Value);
        Assert.Equal(404, problem.Status);
    }

    [Fact]
    public async Task GetPage_ExcessiveOffset_Returns400ValidationProblem()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddControllers();
        using var provider = services.BuildServiceProvider();

        var service = new ProductService(new InMemoryProductRepository());
        var controller = new ProductsV2Controller(service)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = provider
                }
            }
        };
        var request = new ProductPaginationRequest
        {
            PageNumber = int.MaxValue,
            PageSize = 100
        };

        var result = await controller.GetPage(request, CancellationToken.None);

        var response = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(400, response.StatusCode);

        var problem = Assert.IsAssignableFrom<ValidationProblemDetails>(response.Value);
        Assert.Equal(400, problem.Status);
        Assert.Contains("pageNumber", problem.Errors.Keys);
    }

    [Fact]
    public async Task PatchDescription_ExistingProduct_ReturnsUpdatedProductAndPreservesOtherFields()
    {
        const string description = "Controller-updated description";

        var service = new ProductService(new InMemoryProductRepository());
        var controller = new ProductsController(service);
        var id = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var request = new PatchProductDescriptionRequest
        {
            Description = description
        };
        var expected = new ProductResponse
        {
            Id = id,
            Name = "Monitor",
            ImgUri = "https://example.com/products/monitor.png",
            Price = 249.99m,
            Description = description
        };

        var patchResult = await controller.PatchDescription(id, request, CancellationToken.None);

        var patchResponse = Assert.IsAssignableFrom<ObjectResult>(patchResult.Result);
        Assert.Equal(200, patchResponse.StatusCode);

        var patched = Assert.IsType<ProductResponse>(patchResponse.Value);
        Assert.Equivalent(expected, patched, strict: true);

        var readResult = await controller.GetById(id, CancellationToken.None);

        var readResponse = Assert.IsAssignableFrom<ObjectResult>(readResult.Result);
        Assert.Equal(200, readResponse.StatusCode);

        var reread = Assert.IsType<ProductResponse>(readResponse.Value);
        Assert.Equivalent(expected, reread, strict: true);
    }
}
