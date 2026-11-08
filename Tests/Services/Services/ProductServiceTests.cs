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

    public static TheoryData<DateTimeOffset, Guid, int, Guid[], DateTimeOffset, Guid> PaginationCases()
    {
        var earlierDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var laterDate = earlierDate.AddDays(1);
        var keyboardId = Guid.Parse("00000000-0000-0000-0000-000000000003");
        var mouseId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var monitorId = Guid.Parse("00000000-0000-0000-0000-000000000002");

        return new()
        {
            { DateTimeOffset.MinValue, Guid.Empty, 2, [keyboardId, mouseId], laterDate, mouseId },
            { earlierDate, keyboardId, 2, [mouseId, monitorId], laterDate, monitorId },
            { laterDate, mouseId, 2, [monitorId], laterDate, monitorId },
            { laterDate, monitorId, 1, [], laterDate, monitorId }
        };
    }

    [Theory]
    [MemberData(nameof(PaginationCases))]
    public async Task GetPageAsync_ReturnsOrderedPageAndCompleteTotal(
        DateTimeOffset lastDate,
        Guid lastId,
        int pageSize,
        Guid[] expectedIds,
        DateTimeOffset expectedLastDate,
        Guid expectedLastId)
    {
        var service = new ProductService(new InMemoryProductRepository());
        var keySet = new KeySet { LastDate = lastDate, LastId = lastId };

        var result = await service.GetPageAsync(keySet, pageSize, CancellationToken.None);

        Assert.Equal(ResponseStatus.Success, result.Status);

        var page = Assert.IsType<ProductsPageResponse>(result.Data);

        Assert.Equal(expectedLastDate, page.KeySet.LastDate);
        Assert.Equal(expectedLastId, page.KeySet.LastId);
        Assert.Equal(pageSize, page.PageSize);
        Assert.Equal(3L, page.TotalCount);
        Assert.Equal(expectedIds, page.Items.Select(product => product.Id));
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