using Services.Models;

namespace Data.Models;

public static class MockProductData
{
    public static List<Product> Create()
    {
        return
        [
            new Product
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                Name = "Keyboard",
                ImgUri = "https://example.com/products/keyboard.png",
                Price = 79.90m,
                Description = "Mechanical keyboard"
            },
            new Product
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                CreatedAt = new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero),
                Name = "Mouse",
                ImgUri = "https://example.com/products/mouse.png",
                Price = 24.50m,
                Description = null
            },
            new Product
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                CreatedAt = new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero),
                Name = "Monitor",
                ImgUri = "https://example.com/products/monitor.png",
                Price = 249.99m,
                Description = "27-inch monitor"
            }
        ];
    }
}
