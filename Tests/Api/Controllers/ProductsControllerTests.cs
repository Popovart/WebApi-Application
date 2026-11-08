using Api.Controllers;
using Api.Models.Requests;
using Common.Responses;
using Data.Models;
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

    [Theory]
    [InlineData(true, "LastId")]
    [InlineData(false, "LastDate")]
    public async Task GetPage_IncompleteCursor_Returns400ValidationProblemForMissingComponent(bool hasLastDate, string missingComponent)
    {
        await using var services = new ServiceCollection()
            .AddLogging()
            .AddControllers()
            .Services
            .BuildServiceProvider();
        var service = new ProductService(new InMemoryProductRepository());
        var controller = new ProductsV2Controller(service)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = services }
            }
        };
        var request = new ProductPaginationRequest
        {
            LastDate = hasLastDate ? new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero) : null,
            LastId = hasLastDate ? null : Guid.Parse("00000000-0000-0000-0000-000000000001"),
            PageSize = 2
        };

        var result = await controller.GetPage(request, CancellationToken.None);

        var response = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(400, response.StatusCode);

        var problem = Assert.IsType<ValidationProblemDetails>(response.Value);
        Assert.Equal(400, problem.Status);
        Assert.Contains(missingComponent, problem.Errors.Keys);
    }

    [Fact]
    public async Task PatchDescription_ExistingProduct_ReturnsUpdatedProductAndPreservesOtherFields()
    {
        var service = new ProductService(new InMemoryProductRepository());
        var controller = new ProductsController(service);
        var baseline = MockProductData.Create().First();
        var id = baseline.Id;
        var description = $"Controller-updated description: {baseline.Description}";
        var expected = (baseline.Id, baseline.Name, baseline.ImgUri, baseline.Price, Description: description);
        var request = new PatchProductDescriptionRequest
        {
            Description = description
        };

        var patchResult = await controller.PatchDescription(id, request, CancellationToken.None);

        var patchResponse = Assert.IsAssignableFrom<ObjectResult>(patchResult.Result);
        Assert.Equal(200, patchResponse.StatusCode);

        var patched = Assert.IsType<ProductResponse>(patchResponse.Value);
        Assert.Equal(expected, (patched.Id, patched.Name, patched.ImgUri, patched.Price, patched.Description));

        var readResult = await controller.GetById(id, CancellationToken.None);

        var readResponse = Assert.IsAssignableFrom<ObjectResult>(readResult.Result);
        Assert.Equal(200, readResponse.StatusCode);

        var reread = Assert.IsType<ProductResponse>(readResponse.Value);
        Assert.Equal(expected, (reread.Id, reread.Name, reread.ImgUri, reread.Price, reread.Description));
    }
}
