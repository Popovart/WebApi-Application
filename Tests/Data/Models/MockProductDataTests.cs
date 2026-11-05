using Data.Models;

namespace Tests.Data.Models;

public class MockProductDataTests
{
    [Fact]
    public void Create_ReturnsIndependentProductsAndCollections()
    {
        var first = MockProductData.Create();
        var second = MockProductData.Create();

        Assert.Equivalent(first, second, strict: true);

        foreach (var product in first)
        {
            product.Description = "Changed in first fixture";
        }

        first.Clear();

        Assert.Collection(
            second.OrderBy(product => product.Id),
            product => Assert.Null(product.Description),
            product => Assert.Equal("27-inch monitor",product.Description),
            product => Assert.Equal("Mechanical keyboard",product.Description));

        var fresh = MockProductData.Create();
        Assert.Equivalent(second, fresh, strict: true);
    }
}