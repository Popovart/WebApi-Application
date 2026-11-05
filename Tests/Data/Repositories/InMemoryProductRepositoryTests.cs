using Data.Repositories;
using Services.Models;

namespace Tests.Data.Repositories;

public class InMemoryProductRepositoryTests
{
    [Fact]
    public async Task Repositories_DoNotShareDescriptionChanges()
    {
        var first = new InMemoryProductRepository();
        var second = new InMemoryProductRepository();
        var id = Guid.Parse("00000000-0000-0000-0000-000000000002");

        await first.PatchDescriptionAsync( id, "Changed in first repository", CancellationToken.None);
        var unaffected = Assert.IsType<Product>(await second.GetAsync(id, CancellationToken.None));
        Assert.Equal("27-inch monitor", unaffected.Description);
    }
}