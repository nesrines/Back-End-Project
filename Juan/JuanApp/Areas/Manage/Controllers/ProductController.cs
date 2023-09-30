using JuanApp.DataAccessLayer;
using JuanApp.Helpers;
using JuanApp.Models;
using JuanApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JuanApp.Areas.Manage.Controllers;
[Area("manage")]
public class ProductController : Controller
{
    private readonly AppDbContext _context;
    private IWebHostEnvironment _env;
    public ProductController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public IActionResult Index(int currentPage = 1)
    {
        IQueryable<Product> products = _context.Products
            .Include(p => p.Category)
            .Where(p => !p.IsDeleted)
            .OrderByDescending(products => products.Id);

        return View(PaginatedList<Product>.Create(products, currentPage, 5, 5));
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        ViewBag.Categories = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync();

        Console.WriteLine(ModelState.IsValid);
        if (!ModelState.IsValid) return View(product);

        if (product.CategoryId == null || !await _context.Categories.AnyAsync(c => !c.IsDeleted && c.Id == product.CategoryId))
        {
            ModelState.AddModelError("CategoryId", "Category is incorrect.");
            return View(product);
        }


        if (product.Images == null)
        {
            ModelState.AddModelError("Files", "There must be at least 1 image.");
            return View(product);
        }

        if (product.Images.Count() > 10)
        {
            ModelState.AddModelError("Images", "There must be 10 images maximum.");
            return View(product);
        }

        product.ProductImages = new();
        foreach (IFormFile file in product.Images)
        {
            product.ProductImages.Add(new ProductImage { Image = await file.SaveAsync(_env.WebRootPath, "assets", "img", "product") });
        }

        if (product.MainFile == null)
        {
            ModelState.AddModelError("MainFile", "Required.");
            return View(product);
        }
        else product.MainImage = await product.MainFile.SaveAsync(_env.WebRootPath, "assets", "img", "product");

        if (product.Price <= 0)
        {
            ModelState.AddModelError("Price", "Price must be more than $0.");
            return View(product);
        }

        if (product.Discount < 0 || product.Discount > 90)
        {
            ModelState.AddModelError("Discount", "Discount percentage cannot be less than 0 or more than 90.");
            return View(product);
        }

        if (product.Count < 0)
        {
            ModelState.AddModelError("Count", "Count cannot be less than 0.");
            return View(product);
        }

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return BadRequest();

        Product? product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages.Where(pi => !pi.IsDeleted))
            .FirstOrDefaultAsync(p => !p.IsDeleted && p.Id == id);

        if (product == null) return NotFound();

        return View(product);
    }

    public async Task<IActionResult> Update(int? id)
    {
        if (id == null) return BadRequest();

        Product? product = await _context.Products
            .Include(p => p.ProductImages.Where(pi => !pi.IsDeleted))
            .FirstOrDefaultAsync(p => !p.IsDeleted && p.Id == id);

        if (product == null) return NotFound();

        ViewBag.Categories = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync();

        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> Update(int? id, Product product)
    {
        ViewBag.Categories = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync();

        if (id == null || id != product.Id) return BadRequest();

        Product? dbProduct = await _context.Products
            .Include(p => p.ProductImages.Where(pi => !pi.IsDeleted))
            .FirstOrDefaultAsync(p => !p.IsDeleted && p.Id == id);

        if (dbProduct == null) return NotFound();

        product.MainImage = dbProduct.MainImage;
        product.ProductImages = dbProduct.ProductImages;

        if (!ModelState.IsValid) return View(product);

        if (product.Images != null)
        {
            int canUpload = 10 - dbProduct.ProductImages.Count();
            if (product.Images.Count() > canUpload)
            {
                ModelState.AddModelError("Files", $"You can only upload {canUpload} more files.");
                return View(product);
            }
            foreach (IFormFile file in product.Images)
            {
                dbProduct.ProductImages.Add(new ProductImage { Image = await file.SaveAsync(_env.WebRootPath, "assets", "img", "product") });
            }
        }

        if (product.MainFile != null)
        {
            string filePath = Path.Combine(_env.WebRootPath, "assets", "img", "product", dbProduct.MainImage);
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            dbProduct.MainImage = await product.MainFile.SaveAsync(_env.WebRootPath, "assets", "img", "product");
        }


        if (product.CategoryId == null || !await _context.Categories.AnyAsync(c => !c.IsDeleted && c.Id == product.CategoryId))
        {
            ModelState.AddModelError("CategoryId", "Invalid");
            return View(product);
        }

        if (product.Price <= 0)
        {
            ModelState.AddModelError("Price", "Price must be more than $0.");
            return View(product);
        }

        if (product.Discount < 0 || product.Discount > product.Price)
        {
            ModelState.AddModelError("Discount", "Discount percentage cannot be less than 0 or more than 90.");
            return View(product);
        }

        if (product.Count < 0)
        {
            ModelState.AddModelError("Count", "Count cannot be less than 0.");
            return View(product);
        }

        dbProduct.Title = product.Title;
        dbProduct.Price = product.Price;
        dbProduct.Discount = product.Discount;
        dbProduct.Count = product.Count;
        dbProduct.SmallDesc = product.SmallDesc;
        dbProduct.FullDesc = product.FullDesc;
        dbProduct.IsFeatured = product.IsFeatured;
        dbProduct.CategoryId = product.CategoryId;
        dbProduct.UpdatedBy = "User";
        dbProduct.UpdatedDate = DateTime.UtcNow.AddHours(4);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> DeleteImage(int? id, int? imageId)
    {
        if (id == null || imageId == null) return BadRequest();

        Product? product = await _context.Products
            .Include(p => p.ProductImages.Where(pi => !pi.IsDeleted))
            .FirstOrDefaultAsync(p => !p.IsDeleted && p.Id == id);

        if (product == null) return NotFound();

        ProductImage? image = product.ProductImages.FirstOrDefault(pi => pi.Id == imageId);
        if (image == null) return NotFound();

        if (product.ProductImages.Count() == 1) return Conflict();

        image.IsDeleted = true;
        image.DeletedBy = "User";
        image.DeletedDate = DateTime.UtcNow.AddHours(4);

        await _context.SaveChangesAsync();

        string filePath = Path.Combine(_env.WebRootPath, "assets", "img", "product", image.Image);

        if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

        return PartialView("_ProductImagesPartial", product.ProductImages.Where(pi => !pi.IsDeleted));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return BadRequest();

        Product? product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages.Where(pi => !pi.IsDeleted))
            .FirstOrDefaultAsync(p => !p.IsDeleted && p.Id == id);

        if (product == null) return NotFound();

        return View(product);
    }

    public async Task<IActionResult> DeleteProduct(int? id)
    {
        if (id == null) return BadRequest();

        Product? product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages.Where(pi => !pi.IsDeleted))
            .FirstOrDefaultAsync(p => !p.IsDeleted && p.Id == id);

        if (product == null) return NotFound();

        product.IsDeleted = true;

        string filePath = Path.Combine(_env.WebRootPath, "assets", "img", "product", product.MainImage);
        if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

        if (product.ProductImages != null && product.ProductImages.Count() > 0)
        {
            foreach (ProductImage productImage in product.ProductImages)
            {
                productImage.IsDeleted = true;
                filePath = Path.Combine(_env.WebRootPath, "assets", "img", "product", productImage.Image);
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}