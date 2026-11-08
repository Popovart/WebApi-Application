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
