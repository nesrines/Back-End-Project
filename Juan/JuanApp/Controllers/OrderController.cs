namespace JuanApp.Controllers;
[Authorize(Roles ="Member")]
public class OrderController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    public OrderController(AppDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    
    public async Task<IActionResult> CheckOut()
    {
        AppUser? appUser = await _userManager.Users
            .Include(u => u.Addresses.Where(a => !a.IsDeleted && a.IsDefault))
            .Include(u => u.BasketProducts.Where(bp => !bp.IsDeleted)).ThenInclude(bp => bp.Product)
            .FirstOrDefaultAsync(u => u.NormalizedUserName == User.Identity.Name.Trim().ToUpperInvariant());

        if (appUser.BasketProducts == null || !appUser.BasketProducts.Any())
        {
            TempData["Info"] = "Please add products to your basket first.";
            return RedirectToAction("Index", "Product");
        }

        OrderVM orderVM = new()
        {
            Order = new()
            {
                Name = appUser.Name,
                Surname = appUser.Surname,
                Email = appUser.Email,
                Country = appUser.Addresses.First().Country,
                TownCity = appUser.Addresses.First().TownCity,
                Line1 = appUser.Addresses.First().Line1,
                Line2 = appUser.Addresses.First().Line2,
                ZipCode = appUser.Addresses.First().ZipCode
            },
            BasketVMs = appUser.BasketProducts.Select(x => new BasketVM
            {
                Id = (int)x.ProductId,
                Count = x.Count,
                Image = x.Product.MainImage,
                Title = x.Product.Title,
                Price = x.Product.Price * (100 - x.Product.Discount) / 100
            }).ToList()
        };

        return View(orderVM);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(Order order)
    {
        AppUser? appUser = await _userManager.Users
            .Include(u => u.Addresses.Where(a => !a.IsDeleted && a.IsDefault)).Include(u => u.Orders)
            .Include(u => u.BasketProducts.Where(bp => !bp.IsDeleted)).ThenInclude(bp => bp.Product)
            .FirstOrDefaultAsync(u => u.NormalizedUserName == User.Identity.Name.Trim().ToUpperInvariant());

        OrderVM orderVM = new()
        {
            Order = order,
            BasketVMs = appUser.BasketProducts.Select(x => new BasketVM
            {
                Id = (int)x.ProductId,
                Count = x.Count,
                Image = x.Product.MainImage,
                Title = x.Product.Title,
                Price = x.Product.Price * (100 - x.Product.Discount) / 100
            }).ToList()
        };

        if (!ModelState.IsValid) return View(orderVM);
        
        if (appUser.BasketProducts == null || !appUser.BasketProducts.Any())
        {
            TempData["Info"] = "Please add products to your basket first.";
            return RedirectToAction("Index", "Product");
        }

        List<OrderProduct> orderProducts = new();

        foreach (BasketProduct basketProduct in appUser.BasketProducts)
        {
            basketProduct.IsDeleted = true;
            basketProduct.DeletedBy = appUser.UserName;
            basketProduct.DeletedDate = DateTime.UtcNow.AddHours(4);
            orderProducts.Add(new()
            {
                Title = basketProduct.Product.Title,
                Price = basketProduct.Product.Price * (100 - basketProduct.Product.Discount) / 100,
                Count = basketProduct.Count,
                CreatedBy = appUser.UserName
            });
        }

        order.Status = 1;
        order.UserId = appUser.Id;
        order.CreatedBy = appUser.UserName;
        order.OrderProducts = orderProducts;
        order.No = appUser.Orders != null && appUser.Orders.Any() ? appUser.Orders
            .OrderByDescending(o => o.Id).FirstOrDefault().No + 1 : 1;

        Response.Cookies.Append("basket", "");

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Your order has been placed successfully!";

        return RedirectToAction("Index", "Product");
    }
}