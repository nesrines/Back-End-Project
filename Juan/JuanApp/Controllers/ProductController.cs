namespace JuanApp.Controllers;
public class ProductController : Controller
{
    private readonly AppDbContext _context;
    public ProductController(AppDbContext context) { _context = context; }

    public async Task<IActionResult> Index(int? id, int currentPage = 1)
    {
        IQueryable<Product> products = _context.Products
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.Id);
        
        if (id != null && await _context.Categories.AnyAsync(c => !c.IsDeleted && c.Id == id))
            products = products.Where(p => p.CategoryId == id);

        ShopVM shopVM = new()
        {
            Categories = await _context.Categories
            .Include(c => c.Products.Where(p => !p.IsDeleted))
            .Where(c => !c.IsDeleted).ToListAsync(),
            Products = PaginatedList<Product>.Create(products, currentPage, 6, 4)
        };

        return View(shopVM);
    }

    public async Task<IActionResult> CategoryFilter()
    {
        return View(nameof(Index));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return BadRequest();

        Product? product = await _context.Products
            .Include(p => p.ProductImages.Where(pi => !pi.IsDeleted))
            .FirstOrDefaultAsync(p => !p.IsDeleted && p.Id == id);
        
        if (product == null) return NotFound();

        return View(product);
    }

    public async Task<IActionResult> Modal(int? id)
    {
        if (id == null) return BadRequest("Id cannot be null.");

        Product? product = await _context.Products
            .Include(p => p.ProductImages.Where(pi => !pi.IsDeleted))
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return NotFound("Id is incorrect.");

        return PartialView("_ModalPartial", product);
    }

    public async Task<IActionResult> Search(string search, int? categoryId)
    {
        List<Product> products = new List<Product>();
        if (categoryId != null && await _context.Categories.AnyAsync(c => !c.IsDeleted && c.Id == categoryId))
        {
            products = await _context.Products
            .Where(p => !p.IsDeleted && (
            p.Title.ToLower().Contains(search.ToLower()) &&
            p.CategoryId == (int)categoryId)).ToListAsync();
        }
        else
        {
            products = await _context.Products
            .Where(p => !p.IsDeleted && (
            p.Category.Name.ToLower().Contains(search.ToLower()) ||
            p.Title.ToLower().Contains(search.ToLower()))).ToListAsync();
        }

        return PartialView("_SearchPartial", products);
    }
}