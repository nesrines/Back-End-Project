namespace JuanApp.ViewComponents;
public class FeaturedViewComponent : ViewComponent
{
    private readonly AppDbContext _context;
    public FeaturedViewComponent(AppDbContext context) { _context = context; }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        IEnumerable<Product> products = await _context.Products
            .Where(p => !p.IsDeleted && p.Count > 0 && p.IsFeatured).ToListAsync();

        return View(products);
    }
}