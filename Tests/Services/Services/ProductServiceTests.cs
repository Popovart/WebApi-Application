using Common.Models;
using Common.Responses;
using Data.Models;
using Data.Repositories;
using Services.Services;

namespace Tests.Services.Services;

public class ProductServiceTests
{
    [Fact]
    public async Task GetAsync_KnownProduct_ReturnsExpectedProduct()
    {
        var service = new ProductService(new InMemoryProductRepository());
        var expected = CaptureBaseline().First();
        var id = expected.Id;

        var result = await service.GetAsync(id, CancellationToken.None);

        Assert.Equal(ResponseStatus.Success, result.Status);

        var product = Assert.IsType<ProductResponse>(result.Data);

        AssertProduct(expected, product);
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
    [InlineData(1, 2, 0, new[]
    {
        "00000000-0000-0000-0000-000000000001",
        "00000000-0000-0000-0000-000000000002"
    })]
    [InlineData(2, 2, 2, new[]
    {
        "00000000-0000-0000-0000-000000000003"
    })]
    [InlineData(4, 1, 3, new string[] { })]
    [InlineData(3, 2, 4, new string[] { })]
    public async Task GetPageAsync_ReturnsOrderedPageAndCompleteTotal(int pageNumber, int pageSize, int offset, string[] expectedIds)
    {
        var service = new ProductService(new InMemoryProductRepository());

        var result = await service.GetPageAsync(pageNumber, pageSize, offset, CancellationToken.None);

        Assert.Equal(ResponseStatus.Success, result.Status);

        var page = Assert.IsType<ProductsPageResponse>(result.Data);

        Assert.Equal(pageNumber, page.PageNumber);
        Assert.Equal(pageSize, page.PageSize);
        Assert.Equal(3L, page.TotalCount);
        Assert.Equal(expectedIds.Select(Guid.Parse), page.Items.Select(product => product.Id));
    }

    [Fact]
    public async Task PatchDescriptionAsync_ExistingProduct_PersistsDescriptionAndPreservesOtherFields()
    {
        var baseline = CaptureBaseline();
        var service = new ProductService(new InMemoryProductRepository());
        var original = baseline.First();
        var id = original.Id;
        var changedDescription = $"Service-updated description: {original.Description}";

        foreach (var description in new [] { changedDescription, null })
        {
            var expected = original with { Description = description };
            var patchResult = await service.PatchDescriptionAsync(id, description, CancellationToken.None);

            Assert.Equal(ResponseStatus.Success, patchResult.Status);
            AssertProduct(expected, Assert.IsType<ProductResponse>(patchResult.Data));

            var readResult = await service.GetAsync(id, CancellationToken.None);

            Assert.Equal(ResponseStatus.Success, readResult.Status);
            AssertProduct(expected, Assert.IsType<ProductResponse>(readResult.Data));

            var listResult = await service.GetAllAsync(CancellationToken.None);

            Assert.Equal(ResponseStatus.Success, listResult.Status);
            var products = Assert.IsAssignableFrom<IEnumerable<ProductResponse>>(listResult.Data);
            var expectedProducts = baseline.Select(product => product.Id == id ? expected : product).ToArray();
            AssertProducts(expectedProducts, products);
        }
    }

    [Fact]
    public async Task PatchDescriptionAsync_MissingProduct_ReturnsNotFoundWithoutChangingProducts()
    {
        var service = new ProductService(new InMemoryProductRepository());
        var missingId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");

        var expected = CaptureBaseline();

        var patchResult = await service.PatchDescriptionAsync(missingId, "Must not be inserted", CancellationToken.None);

        Assert.Equal(ResponseStatus.NotFound, patchResult.Status);
        Assert.Null(patchResult.Data);

        var afterResult = await service.GetAllAsync(CancellationToken.None);
        Assert.Equal(ResponseStatus.Success, afterResult.Status);

        var after = Assert.IsAssignableFrom<IEnumerable<ProductResponse>>(afterResult.Data);
        AssertProducts(expected, after);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsExpectedProducts()
    {
        var service = new ProductService(new InMemoryProductRepository());
        var expected = CaptureBaseline();

        var result = await service.GetAllAsync(CancellationToken.None);

        Assert.Equal(ResponseStatus.Success, result.Status);

        var products = Assert.IsAssignableFrom<IEnumerable<ProductResponse>>(result.Data);
        AssertProducts(expected, products);
    }

    private static ProductSnapshot[] CaptureBaseline()
    {
        return MockProductData.Create()
            .Select(product => new ProductSnapshot(
                product.Id, product.Name, product.ImgUri, product.Price, product.Description))
            .ToArray();
    }

    private static void AssertProduct(ProductSnapshot expected, ProductResponse actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.ImgUri, actual.ImgUri);
        Assert.Equal(expected.Price, actual.Price);
        Assert.Equal(expected.Description, actual.Description);
    }

    private static void AssertProducts(ProductSnapshot[] expected, IEnumerable<ProductResponse> actual)
    {
        var products = actual.ToArray();
        Assert.Equal(expected.Length, products.Length);

        foreach (var baseline in expected)
        {
            AssertProduct(baseline, Assert.Single(products, product => product.Id == baseline.Id));
        }
    }

    private readonly record struct ProductSnapshot(
        Guid Id, string Name, string ImgUri, decimal Price, string? Description);
}