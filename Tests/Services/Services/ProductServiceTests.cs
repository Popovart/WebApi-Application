using Common.Models;
using Common.Responses;
using Data.Repositories;
using Services.Services;

namespace Tests.Services.Services;

public class ProductServiceTests
{
    [Fact]
    public async Task GetAsync_KnownProduct_ReturnsExpectedProduct()
    {
        var service = new ProductService(new InMemoryProductRepository());
        var id = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var result = await service.GetAsync(id, CancellationToken.None);

        Assert.Equal(ResponseStatus.Success, result.Status);

        var product = Assert.IsType<ProductResponse>(result.Data);

        Assert.Equal(id, product.Id);
        Assert.Equal("Mouse", product.Name);
        Assert.Equal("https://example.com/products/mouse.png", product.ImgUri);
        Assert.Equal(24.50m, product.Price);
        Assert.Null(product.Description);
    }

    [Fact]
    public async Task GetAsync_MissingProduct_ReturnsNotFound()
    {
        var service = new ProductService(new InMemoryProductRepository());
        var id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");

        var result = await service.GetAsync(id, CancellationToken.None);

        Assert.Equal(ResponseStatus.NotFound, result.Status);
        Assert.Null(result.Data);
    }

    [Theory]
    [InlineData(1, new[]
    {
        "00000000-0000-0000-0000-000000000001",
        "00000000-0000-0000-0000-000000000002"
    })]
    [InlineData(2, new[]
    {
        "00000000-0000-0000-0000-000000000003"
    })]
    [InlineData(3, new string[] { })]
    public async Task GetPageAsync_ReturnsOrderedPageAndCompleteTotal(int pageNumber, string[] expectedIds)
    {
        const int pageSize = 2;

        var service = new ProductService(new InMemoryProductRepository());
        var offset = (pageNumber - 1) * pageSize;

        var result = await service.GetPageAsync(pageNumber, pageSize, offset, CancellationToken.None);

        Assert.Equal(ResponseStatus.Success, result.Status);

        var page = Assert.IsType<ProductsPageResponse>(result.Data);

        Assert.Equal(pageNumber, page.PageNumber);
        Assert.Equal(pageSize, page.PageSize);
        Assert.Equal(3L, page.TotalCount);
        Assert.Equal(expectedIds.Select(Guid.Parse), page.Items.Select(product => product.Id));
    }

    [Theory]
    [InlineData("Updated description")]
    [InlineData(null)]
    public async Task PatchDescriptionAsync_ExistingProduct_PersistsDescriptionAndPreservesOtherFields(string? description)
    {
        var service = new ProductService(new InMemoryProductRepository());
        var id = Guid.Parse("00000000-0000-0000-0000-000000000002");

        var originalResult = await service.GetAsync(id, CancellationToken.None);

        Assert.Equal(ResponseStatus.Success, originalResult.Status);

        var original = Assert.IsType<ProductResponse>(originalResult.Data);

        Assert.Equal("27-inch monitor", original.Description);

        var patchResult = await service.PatchDescriptionAsync(id, description, CancellationToken.None);

        Assert.Equal(ResponseStatus.Success, patchResult.Status);

        var patched = Assert.IsType<ProductResponse>(patchResult.Data);

        var readResult = await service.GetAsync(id, CancellationToken.None);

        Assert.Equal(ResponseStatus.Success, readResult.Status);
        var reread = Assert.IsType<ProductResponse>(readResult.Data);
        foreach (var product in new[] { patched, reread })
        {
            Assert.Equal(description, product.Description);
            Assert.Equal(id, product.Id);
            Assert.Equal(original.Name, product.Name);
            Assert.Equal(original.ImgUri, product.ImgUri);
            Assert.Equal(original.Price, product.Price);
        }
    }

    [Fact]
    public async Task PatchDescriptionAsync_MissingProduct_ReturnsNotFoundWithoutChangingProducts()
    {
        var service = new ProductService(new InMemoryProductRepository());
        var missingId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");

        var beforeResult = await service.GetAllAsync(CancellationToken.None);

        Assert.Equal(ResponseStatus.Success, beforeResult.Status);

        var before = Assert.IsAssignableFrom<IEnumerable<ProductResponse>>(beforeResult.Data).ToArray();

        var patchResult = await service.PatchDescriptionAsync(missingId, "Must not be inserted", CancellationToken.None);

        Assert.Equal(ResponseStatus.NotFound, patchResult.Status);
        Assert.Null(patchResult.Data);

        var afterResult = await service.GetAllAsync(CancellationToken.None);
        Assert.Equal(ResponseStatus.Success, afterResult.Status);

        var after = Assert.IsAssignableFrom<IEnumerable<ProductResponse>>(afterResult.Data).ToArray();
        Assert.Equivalent(before, after, strict: true);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsExpectedProducts()
    {
        var service = new ProductService(new InMemoryProductRepository());

        var result = await service.GetAllAsync(CancellationToken.None);

        Assert.Equal(ResponseStatus.Success, result.Status);

        var products = Assert.IsAssignableFrom<IEnumerable<ProductResponse>>(result.Data).ToArray();
        ProductResponse[] expected =
        [
            new ProductResponse
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Name = "Mouse",
                ImgUri = "https://example.com/products/mouse.png",
                Price = 24.50m,
                Description = null
            },
            new ProductResponse
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Name = "Monitor",
                ImgUri = "https://example.com/products/monitor.png",
                Price = 249.99m,
                Description = "27-inch monitor"
            },
            new ProductResponse
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Name = "Keyboard",
                ImgUri = "https://example.com/products/keyboard.png",
                Price = 79.90m,
                Description = "Mechanical keyboard"
            }
        ];

        Assert.Equivalent(expected, products, strict: true);
    }
}