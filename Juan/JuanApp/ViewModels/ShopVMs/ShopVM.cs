using JuanApp.Models;

namespace JuanApp.ViewModels.ShopVMs;
public class ShopVM
{
    public IEnumerable<Category> Categories { get; set; }
    public PaginatedList<Product> Products { get; set; }
}