using Common.Models;
using Common.Responses;
using Data.Repositories;
using Services.Services;
using Services.Models;

namespace Tests.Services.Services;

public class ProductServiceTests
{
    private static readonly Guid KeyboardId = Guid.Parse("00000000-0000-0000-0000-000000000003");
    private static readonly Guid MouseId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid MonitorId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly DateTimeOffset KeyboardDate =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset MouseAndMonitorDate =
        new(2026, 1, 2, 0, 0, 0, TimeSpan.Zero);

    private static readonly Guid SqlFirstId = Guid.Parse("00000001-0000-0000-0000-000000000000");
    private static readonly Guid SqlSecondId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid AdjacentTickId = Guid.Empty;
    private static readonly Guid LaterId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly DateTimeOffset SqlScenarioTime =
        new DateTimeOffset(2026, 4, 5, 6, 7, 8, TimeSpan.Zero).AddTicks(1_234_567);

    [Fact]
    public async Task GetAsync_KnownProduct_ReturnsExpectedProduct()
    {
        var service = new ProductService(new InMemoryProductRepository());
        var expected = CaptureBaseline().First();
        var id = expected.Id;

        var result = await service.GetAsync(id, CancellationToken.None);

        Assert.Equal(ResponseStatus.Success, result.Status);

        AssertProduct(expected, result.Data);
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

    [Fact]
    public async Task GetPageAsync_ThreeProducts_ReturnsProbeOnFinalPartialPage()
    {
        var service = new ProductService(new InMemoryProductRepository());

        var firstPage = AssertPage(
            await service.GetPageAsync(null, 2, CancellationToken.None),
            2,
            KeyboardId,
            MouseId);
        AssertCursor(firstPage.NextCursor, MouseAndMonitorDate, MouseId);

        var secondPage = AssertPage(
            await service.GetPageAsync(firstPage.NextCursor, 2, CancellationToken.None),
            2,
            MonitorId);
        Assert.Null(secondPage.NextCursor);

        Assert.Equal(
            new[] { KeyboardId, MouseId, MonitorId },
            firstPage.Items.Concat(secondPage.Items).Select(product => product.Id));
    }

    [Fact]
    public async Task GetPageAsync_ArbitraryGuidsAndAdjacentTicks_FollowsSqlSortAndSeek()
    {
        var service = new ProductService(new InMemoryProductRepository(CreateSqlOrderScenario()));

        var firstPage = AssertPage(
            await service.GetPageAsync(null, 2, CancellationToken.None),
            2,
            SqlFirstId,
            SqlSecondId);
        AssertCursor(firstPage.NextCursor, SqlScenarioTime, SqlSecondId);

        var secondPage = AssertPage(
            await service.GetPageAsync(firstPage.NextCursor, 2, CancellationToken.None),
            2,
            AdjacentTickId,
            LaterId);
        Assert.Null(secondPage.NextCursor);
        Assert.Equal(SqlScenarioTime.AddTicks(1), secondPage.Items[0].CreatedAt);
        Assert.Equal(SqlScenarioTime.AddTicks(2), secondPage.Items[1].CreatedAt);
    }

    [Fact]
    public async Task GetPageAsync_ContinuationMayChangePageSize()
    {
        var service = new ProductService(new InMemoryProductRepository(CreateSqlOrderScenario()));

        var firstPage = AssertPage(
            await service.GetPageAsync(null, 1, CancellationToken.None),
            1,
            SqlFirstId);
        AssertCursor(firstPage.NextCursor, SqlScenarioTime, SqlFirstId);

        var secondPage = AssertPage(
            await service.GetPageAsync(firstPage.NextCursor, 2, CancellationToken.None),
            2,
            SqlSecondId,
            AdjacentTickId);
        AssertCursor(secondPage.NextCursor, SqlScenarioTime.AddTicks(1), AdjacentTickId);

        var finalPage = AssertPage(
            await service.GetPageAsync(secondPage.NextCursor, 3, CancellationToken.None),
            3,
            LaterId);
        Assert.Null(finalPage.NextCursor);
    }

    [Fact]
    public async Task GetPageAsync_DeletedBoundary_ContinuesFromSavedValues()
    {
        var firstService = new ProductService(new InMemoryProductRepository(CreateSqlOrderScenario()));
        var firstPage = AssertPage(
            await firstService.GetPageAsync(null, 2, CancellationToken.None),
            2,
            SqlFirstId,
            SqlSecondId);
        var savedCursor = Assert.IsType<ProductCursor>(firstPage.NextCursor);

        var fixtureWithoutBoundary = CreateSqlOrderScenario()
            .Where(product => product.Id != SqlSecondId)
            .ToArray();
        var continuedService = new ProductService(new InMemoryProductRepository(fixtureWithoutBoundary));

        var continuedPage = AssertPage(
            await continuedService.GetPageAsync(savedCursor, 2, CancellationToken.None),
            2,
            AdjacentTickId,
            LaterId);
        Assert.Null(continuedPage.NextCursor);
    }

    [Fact]
    public async Task GetPageAsync_EmptyInitialPage_HasNoCursor()
    {
        var service = new ProductService(new InMemoryProductRepository(Array.Empty<Product>()));

        var page = AssertPage(
            await service.GetPageAsync(null, 3, CancellationToken.None),
            3);

        Assert.Null(page.NextCursor);
    }

    [Fact]
    public async Task GetPageAsync_AtAndBeyondEnd_ReturnEmptyPagesWithoutPreservingInput()
    {
        var service = new ProductService(new InMemoryProductRepository());
        ProductCursor[] boundaries =
        [
            new() { LastDate = MouseAndMonitorDate, LastId = MonitorId },
            new() { LastDate = MouseAndMonitorDate.AddTicks(1), LastId = Guid.Empty }
        ];

        foreach (var boundary in boundaries)
        {
            var page = AssertPage(
                await service.GetPageAsync(boundary, 2, CancellationToken.None),
                2);
            Assert.Null(page.NextCursor);
        }
    }

    private static Product[] CreateSqlOrderScenario()
    {
        return
        [
            new Product
            {
                Id = AdjacentTickId,
                CreatedAt = SqlScenarioTime.AddTicks(1),
                Name = "Adjacent tick",
                ImgUri = "https://example.com/scenarios/adjacent.png",
                Price = 30.03m,
                Description = "Later date with the smallest ID"
            },
            new Product
            {
                Id = SqlSecondId,
                CreatedAt = SqlScenarioTime,
                Name = "SQL second",
                ImgUri = "https://example.com/scenarios/second.png",
                Price = 20.02m,
                Description = null
            },
            new Product
            {
                Id = LaterId,
                CreatedAt = SqlScenarioTime.AddTicks(2),
                Name = "Later",
                ImgUri = "https://example.com/scenarios/later.png",
                Price = 40.04m,
                Description = "Last product"
            },
            new Product
            {
                Id = SqlFirstId,
                CreatedAt = SqlScenarioTime,
                Name = "SQL first",
                ImgUri = "https://example.com/scenarios/first.png",
                Price = 10.01m,
                Description = "First under SQL Server GUID order"
            }
        ];
    }

    private static ProductPageResult AssertPage(
        ResponseResult<ProductPageResult> result,
        int expectedPageSize,
        params Guid[] expectedIds)
    {
        Assert.Equal(ResponseStatus.Success, result.Status);
        var page = Assert.IsType<ProductPageResult>(result.Data);
        Assert.Equal(expectedPageSize, page.PageSize);
        Assert.Equal(expectedIds, page.Items.Select(product => product.Id));
        return page;
    }

    private static void AssertCursor(
        ProductCursor? cursor,
        DateTimeOffset expectedLastDate,
        Guid expectedLastId)
    {
        Assert.NotNull(cursor);
        Assert.Equal(expectedLastDate, cursor.LastDate);
        Assert.Equal(expectedLastId, cursor.LastId);
    }


    [Fact]
    public async Task PatchDescriptionAsync_DescriptionSetThenCleared_UpdatesAllReads()
    {
        var baseline = CaptureBaseline();
        var service = new ProductService(new InMemoryProductRepository());
        var originalProduct = baseline.First();
        var changedDescription = $"Service-updated description: {originalProduct.Description}";
        var expectedUpdatedProduct = originalProduct with { Description = changedDescription };

        var updateResult = await service.PatchDescriptionAsync(
            originalProduct.Id, changedDescription, CancellationToken.None);

        Assert.Equal(ResponseStatus.Success, updateResult.Status);
        AssertProduct(expectedUpdatedProduct, updateResult.Data);
        await AssertLookupAndListAsync(service, baseline, expectedUpdatedProduct);

        var expectedClearedProduct = originalProduct with { Description = null };

        var clearResult = await service.PatchDescriptionAsync(
            originalProduct.Id, null, CancellationToken.None);

        Assert.Equal(ResponseStatus.Success, clearResult.Status);
        AssertProduct(expectedClearedProduct, clearResult.Data);
        await AssertLookupAndListAsync(service, baseline, expectedClearedProduct);
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

        AssertProducts(expected, afterResult.Data);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsExpectedProducts()
    {
        var service = new ProductService(new InMemoryProductRepository());
        var expected = CaptureBaseline();

        var result = await service.GetAllAsync(CancellationToken.None);

        Assert.Equal(ResponseStatus.Success, result.Status);

        AssertProducts(expected, result.Data);
    }

    private static ProductSnapshot[] CaptureBaseline()
    {
        return
        [
            new ProductSnapshot(
                KeyboardId,
                "Keyboard",
                KeyboardDate,
                "https://example.com/products/keyboard.png",
                79.90m,
                "Mechanical keyboard"),
            new ProductSnapshot(
                MouseId,
                "Mouse",
                MouseAndMonitorDate,
                "https://example.com/products/mouse.png",
                24.50m,
                null),
            new ProductSnapshot(
                MonitorId,
                "Monitor",
                MouseAndMonitorDate,
                "https://example.com/products/monitor.png",
                249.99m,
                "27-inch monitor")
        ];
    }

    private static async Task AssertLookupAndListAsync(
        ProductService service, ProductSnapshot[] baseline, ProductSnapshot expectedProduct)
    {
        var readResult = await service.GetAsync(expectedProduct.Id, CancellationToken.None);

        Assert.Equal(ResponseStatus.Success, readResult.Status);
        AssertProduct(expectedProduct, readResult.Data);

        var listResult = await service.GetAllAsync(CancellationToken.None);

        Assert.Equal(ResponseStatus.Success, listResult.Status);
        var expectedProducts = baseline
            .Select(product => product.Id == expectedProduct.Id ? expectedProduct : product)
            .ToArray();
        AssertProducts(expectedProducts, listResult.Data);
    }

    private static void AssertProduct(ProductSnapshot expected, ProductResponse? actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.CreatedAt, actual.CreatedAt);
        Assert.Equal(expected.ImgUri, actual.ImgUri);
        Assert.Equal(expected.Price, actual.Price);
        Assert.Equal(expected.Description, actual.Description);
    }

    private static void AssertProducts(ProductSnapshot[] expected, IEnumerable<ProductResponse>? actual)
    {
        Assert.NotNull(actual);
        var products = actual.ToArray();
        Assert.Equal(expected.Length, products.Length);

        foreach (var baseline in expected)
        {
            AssertProduct(baseline, Assert.Single(products, product => product.Id == baseline.Id));
        }
    }

    private readonly record struct ProductSnapshot(
        Guid Id, string Name, DateTimeOffset CreatedAt, string ImgUri, decimal Price, string? Description);
}