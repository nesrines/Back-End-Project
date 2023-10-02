using JuanApp.DataAccessLayer;
using JuanApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JuanApp.ViewComponents;
public class TopSellerViewComponent : ViewComponent
{
    private readonly AppDbContext _context;
    public TopSellerViewComponent(AppDbContext context) { _context = context; }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        IEnumerable<Product> products = await _context.Products
            .Where(p => !p.IsDeleted && p.Count > 0).ToListAsync();

        return View(products);
    }
}