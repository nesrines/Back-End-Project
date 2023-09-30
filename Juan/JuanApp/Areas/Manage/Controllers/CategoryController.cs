using JuanApp.DataAccessLayer;
using JuanApp.Models;
using JuanApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JuanApp.Areas.Manage.Controllers;
[Area("manage")]
public class CategoryController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;
    public CategoryController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public IActionResult Index(int currentPage = 1)
    {
        IQueryable<Category> categories = _context.Categories
            .Include(c => c.Products.Where(p => !p.IsDeleted))
            .Where(c => !c.IsDeleted).OrderByDescending(c => c.Id);

        return View(PaginatedList<Category>.Create(categories, currentPage, 5, 5));
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Category category)
    {
        if (!ModelState.IsValid) return View(category);

        if (await _context.Categories.AnyAsync(c => !c.IsDeleted && c.Name.ToLower() == category.Name.Trim().ToLower()))
        {
            ModelState.AddModelError("Name", $"{category.Name.Trim()} already exists in the Database.");
            return View(category);
        }
        category.Name = category.Name.Trim();


        category.CreatedBy = "User";
        category.CreatedDate = DateTime.UtcNow.AddHours(4);
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return BadRequest();
        Category? category = await _context.Categories
            .Include(c => c.Products.Where(p => !p.IsDeleted))
            .FirstOrDefaultAsync(b => !b.IsDeleted && b.Id == id);

        if (category == null) return NotFound();

        return View(category);
    }

    public async Task<IActionResult> Update(int? id)
    {
        if (id == null) return BadRequest();
        Category? category = await _context.Categories.FirstOrDefaultAsync(c => !c.IsDeleted && c.Id == id);
        if (category == null) return NotFound();
        return View(category);
    }

    [HttpPost]
    public async Task<IActionResult> Update(int? id, Category category)
    {
        if (id == null || id != category.Id) return BadRequest();

        if (!ModelState.IsValid) return View(category);

        Category? dbCategory = await _context.Categories.FirstOrDefaultAsync(c => !c.IsDeleted && c.Id == id);

        if (dbCategory == null) return NotFound();

        if (await _context.Categories.AnyAsync(c => !c.IsDeleted && c.Name.ToLower() == category.Name.Trim().ToLower() && c.Id != id))
        {
            ModelState.AddModelError("Name", $"{category.Name.Trim()} already exists in the Database");
            return View(category);
        }
        dbCategory.Name = category.Name.Trim();

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return BadRequest();

        Category? category = await _context.Categories
            .Include(b => b.Products.Where(p => !p.IsDeleted))
            .FirstOrDefaultAsync(b => !b.IsDeleted && b.Id == id);

        if (category == null) return NotFound();

        ViewBag.CannotDelete = category.Products != null && category.Products.Count() > 0;

        return View(category);
    }

    public async Task<IActionResult> DeleteCategory(int? id)
    {
        if (id == null) return BadRequest();

        Category? category = await _context.Categories
            .Include(b => b.Products.Where(p => !p.IsDeleted))
            .FirstOrDefaultAsync(b => !b.IsDeleted && b.Id == id);

        if (category == null) return NotFound();

        if (category.Products != null && category.Products.Count() > 0) return BadRequest();

        category.IsDeleted = true;
        category.DeletedBy = "User";
        category.DeletedDate = DateTime.UtcNow.AddHours(4);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}