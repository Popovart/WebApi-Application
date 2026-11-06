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
            .Select(product => (product.Id, product.Name, product.ImgUri, product.Price, product.Description))
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

    private static void AssertProducts(
        (Guid Id, string Name, string ImgUri, decimal Price, string? Description)[] expected,
        IReadOnlyCollection<Product> products)
    {
        Assert.Equal(expected.Length, products.Count);

        foreach (var baseline in expected)
        {
            var product = Assert.Single(products, product => product.Id == baseline.Id);
            Assert.Equal(baseline, (product.Id, product.Name, product.ImgUri, product.Price, product.Description));
        }
    }
}