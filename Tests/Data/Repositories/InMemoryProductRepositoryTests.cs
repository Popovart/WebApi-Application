using Data.Repositories;
using Services.Models;

namespace Tests.Data.Repositories;

public class InMemoryProductRepositoryTests
{
    [Fact]
    public async Task Repositories_DoNotShareDescriptionChanges()
    {
        var updatedRepository = new InMemoryProductRepository();
        var existingRepository = new InMemoryProductRepository();
        var baseline = new ProductSnapshot(
            Guid.Parse("00000000-0000-0000-0000-000000000003"),
            "Keyboard",
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            "https://example.com/products/keyboard.png",
            79.90m,
            "Mechanical keyboard");
        var id = baseline.Id;
        var description = $"Repository-updated description: {baseline.Description}";

        await updatedRepository.PatchDescriptionAsync(id, description, CancellationToken.None);

        var updatedProduct = await updatedRepository.GetAsync(id, CancellationToken.None);
        AssertProduct(baseline with { Description = description }, updatedProduct);

        var existingRepositoryProduct = await existingRepository.GetAsync(id, CancellationToken.None);
        AssertProduct(baseline, existingRepositoryProduct);

        var repositoryCreatedAfterUpdate = new InMemoryProductRepository();
        var newRepositoryProduct = await repositoryCreatedAfterUpdate.GetAsync(id, CancellationToken.None);
        AssertProduct(baseline, newRepositoryProduct);
    }

    [Fact]
    public async Task ScenarioInputAndReturnedPage_AreIndependentSnapshots()
    {
        var product = new Product
        {
            Id = Guid.Parse("12345678-90ab-cdef-1234-567890abcdef"),
            CreatedAt = new DateTimeOffset(2026, 6, 7, 8, 9, 10, TimeSpan.Zero).AddTicks(7),
            Name = "Scenario product",
            ImgUri = "https://example.com/scenarios/product.png",
            Price = 123.45m,
            Description = "Original description"
        };
        var expected = new ProductSnapshot(
            product.Id,
            product.Name,
            product.CreatedAt,
            product.ImgUri,
            product.Price,
            product.Description);
        var input = new List<Product> { product };
        var repository = new InMemoryProductRepository(input);

        product.Id = Guid.Empty;
        product.CreatedAt = DateTimeOffset.MinValue;
        product.Name = "Changed input";
        product.ImgUri = "https://invalid.example/changed-input.png";
        product.Price = 0m;
        product.Description = null;
        input.Clear();

        var firstPage = await repository.GetPageAsync(null, 1, CancellationToken.None);
        AssertProduct(expected, Assert.Single(firstPage));

        firstPage[0].Id = Guid.Empty;
        firstPage[0].CreatedAt = DateTimeOffset.MaxValue;
        firstPage[0].Name = "Changed result";
        firstPage[0].ImgUri = "https://invalid.example/changed-result.png";
        firstPage[0].Price = -1m;
        firstPage[0].Description = "Changed result";

        var secondPage = await repository.GetPageAsync(null, 1, CancellationToken.None);
        AssertProduct(expected, Assert.Single(secondPage));
    }

    private static void AssertProduct(ProductSnapshot expected, Product? actual)
    {
        Assert.NotNull(actual);
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