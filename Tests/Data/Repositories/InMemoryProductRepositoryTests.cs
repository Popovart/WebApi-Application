using Data.Models;
using Data.Repositories;

namespace Tests.Data.Repositories;

public class InMemoryProductRepositoryTests
{
    [Fact]
    public async Task Repositories_DoNotShareDescriptionChanges()
    {
        var first = new InMemoryProductRepository();
        var second = new InMemoryProductRepository();
        var baseline = MockProductData.Create()
            .Select(product => (product.Id, product.Name, product.ImgUri, product.Price, product.Description))
            .First();
        var id = baseline.Id;
        var description = $"Changed in first repository: {baseline.Description}";

        await first.PatchDescriptionAsync(id, description, CancellationToken.None);

        var changed = await first.GetAsync(id, CancellationToken.None);
        Assert.NotNull(changed);
        Assert.Equal(
            (baseline.Id, baseline.Name, baseline.ImgUri, baseline.Price, Description: description),
            (changed.Id, changed.Name, changed.ImgUri, changed.Price, changed.Description));

        var unaffected = await second.GetAsync(id, CancellationToken.None);
        Assert.NotNull(unaffected);
        Assert.Equal(baseline, (unaffected.Id, unaffected.Name, unaffected.ImgUri, unaffected.Price, unaffected.Description));

        var third = new InMemoryProductRepository();
        var fresh = await third.GetAsync(id, CancellationToken.None);
        Assert.NotNull(fresh);
        Assert.Equal(baseline, (fresh.Id, fresh.Name, fresh.ImgUri, fresh.Price, fresh.Description));
    }
}