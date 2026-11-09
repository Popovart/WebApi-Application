using Api.Controllers;
using Api.Pagination;
using Api.Models.Requests;
using Common.Models;
using Common.Responses;
using Data.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Services.Services;
using Services.Interfaces;
using Services.Models;

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
    [InlineData("invalid=")]
    [InlineData("CAASEAAAAAAAAAAAAAAAAAAAAAA")]
    public async Task GetPage_InvalidCursor_ReturnsKeyed400BeforeServiceAccess(string token)
    {
        await using var services = new ServiceCollection()
            .AddLogging()
            .AddControllers()
            .Services
            .BuildServiceProvider();
        var controller = new ProductsV2Controller(new ThrowingProductService())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = services }
            }
        };
        var request = new ProductPaginationRequest
        {
            Cursor = token,
            PageSize = 2
        };

        var result = await controller.GetPage(request, CancellationToken.None);

        var response = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(400, response.StatusCode);

        var problem = Assert.IsType<ValidationProblemDetails>(response.Value);
        Assert.Equal(400, problem.Status);
        var cursorError = Assert.Single(problem.Errors);
        Assert.Equal("cursor", cursorError.Key);
        Assert.NotEmpty(cursorError.Value);
        Assert.All(cursorError.Value, message => Assert.False(string.IsNullOrWhiteSpace(message)));
    }

    [Fact]
    public async Task GetPage_InitialAndReplayedToken_ReturnMeaningfulPages()
    {
        var service = new ProductService(new InMemoryProductRepository());
        var controller = new ProductsV2Controller(service);

        var firstResult = await controller.GetPage(
            new ProductPaginationRequest { Cursor = null, PageSize = 2 },
            CancellationToken.None);

        var firstPage = AssertPageResponse(firstResult);
        Assert.Equal(2, firstPage.PageSize);
        Assert.Equal(
            new[]
            {
                Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Guid.Parse("00000000-0000-0000-0000-000000000001")
            },
            firstPage.Items.Select(product => product.Id));
        AssertProduct(
            new ProductSnapshot(
                Guid.Parse("00000000-0000-0000-0000-000000000003"),
                "Keyboard",
                new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                "https://example.com/products/keyboard.png",
                79.90m,
                "Mechanical keyboard"),
            firstPage.Items[0]);

        var token = Assert.IsType<string>(firstPage.NextCursor);
        Assert.True(ProductCursorCodec.TryDecode(token, out var decodedCursor));
        Assert.NotNull(decodedCursor);
        Assert.Equal(
            new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero),
            decodedCursor.LastDate);
        Assert.Equal(
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            decodedCursor.LastId);

        var secondResult = await controller.GetPage(
            new ProductPaginationRequest { Cursor = token, PageSize = 2 },
            CancellationToken.None);

        var secondPage = AssertPageResponse(secondResult);
        Assert.Equal(2, secondPage.PageSize);
        Assert.Equal(
            new[] { Guid.Parse("00000000-0000-0000-0000-000000000002") },
            secondPage.Items.Select(product => product.Id));
        Assert.Null(secondPage.NextCursor);
    }

    [Fact]
    public async Task PatchDescription_ExistingProduct_ReturnsUpdatedProductAndPreservesOtherFields()
    {
        var service = new ProductService(new InMemoryProductRepository());
        var controller = new ProductsController(service);
        var id = Guid.Parse("00000000-0000-0000-0000-000000000003");
        var description = "Controller-updated description: Mechanical keyboard";
        var expected = new ProductSnapshot(
            id,
            "Keyboard",
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            "https://example.com/products/keyboard.png",
            79.90m,
            description);
        var request = new PatchProductDescriptionRequest
        {
            Description = description
        };

        var patchResult = await controller.PatchDescription(id, request, CancellationToken.None);

        var patchResponse = Assert.IsAssignableFrom<ObjectResult>(patchResult.Result);
        Assert.Equal(200, patchResponse.StatusCode);

        var patched = Assert.IsType<ProductResponse>(patchResponse.Value);
        AssertProduct(expected, patched);

        var readResult = await controller.GetById(id, CancellationToken.None);

        var readResponse = Assert.IsAssignableFrom<ObjectResult>(readResult.Result);
        Assert.Equal(200, readResponse.StatusCode);

        var reread = Assert.IsType<ProductResponse>(readResponse.Value);
        AssertProduct(expected, reread);
    }

    private static ProductsPageResponse AssertPageResponse(
        ActionResult<ProductsPageResponse> actionResult)
    {
        Assert.Null(actionResult.Result);
        return Assert.IsType<ProductsPageResponse>(actionResult.Value);
    }

    private sealed class ThrowingProductService : IProductService
    {
        public Task<ResponseResult<IEnumerable<ProductResponse>>> GetAllAsync(
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<ResponseResult<ProductPageResult>> GetPageAsync(
            ProductCursor? cursor,
            int pageSize,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Invalid cursor reached the product service.");

        public Task<ResponseResult<ProductResponse>> GetAsync(
            Guid id,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<ResponseResult<ProductResponse>> PatchDescriptionAsync(
            Guid id,
            string? description,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private static void AssertProduct(ProductSnapshot expected, ProductResponse actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.CreatedAt, actual.CreatedAt);
        Assert.Equal(expected.ImgUri, actual.ImgUri);
        Assert.Equal(expected.Price, actual.Price);
        Assert.Equal(expected.Description, actual.Description);
    }

    private readonly record struct ProductSnapshot(
        Guid Id, string Name, DateTimeOffset CreatedAt, string ImgUri, decimal Price, string? Description);
}
