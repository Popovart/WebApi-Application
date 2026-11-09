using Data.Models;
using Services.Models;

namespace Tests.Data.Models;

public class MockProductDataTests
{
    [Fact]
    public void Create_ReturnsIndependentProductsAndCollections()
    {
        var first = MockProductData.Create();
        var second = MockProductData.Create();
        var expected = first
            .Select(product => new ProductSnapshot(
                product.Id, product.Name, product.CreatedAt, product.ImgUri, product.Price, product.Description))
            .ToArray();

        Assert.Equivalent(first, second, strict: true);

        foreach (var product in first)
        {
            product.Description = $"Changed in first fixture: {product.Description}";
        }

        first.Clear();

        AssertProducts(expected, second);

        var fresh = MockProductData.Create();
        AssertProducts(expected, fresh);
    }

    private static void AssertProducts(ProductSnapshot[] expected, IReadOnlyCollection<Product> products)
    {
        Assert.Equal(expected.Length, products.Count);

        foreach (var expectedProduct in expected)
        {
            var actualProduct = Assert.Single(products, product => product.Id == expectedProduct.Id);
            Assert.Equal(expectedProduct.Id, actualProduct.Id);
            Assert.Equal(expectedProduct.Name, actualProduct.Name);
            Assert.Equal(expectedProduct.CreatedAt, actualProduct.CreatedAt);
            Assert.Equal(expectedProduct.ImgUri, actualProduct.ImgUri);
            Assert.Equal(expectedProduct.Price, actualProduct.Price);
            Assert.Equal(expectedProduct.Description, actualProduct.Description);
        }
    }

    private readonly record struct ProductSnapshot(
        Guid Id, string Name, DateTimeOffset CreatedAt, string ImgUri, decimal Price, string? Description);
}