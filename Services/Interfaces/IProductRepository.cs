using Services.Models;

namespace Services.Interfaces;

public interface IProductRepository
{
    IReadOnlyList<Product> GetAll();
}