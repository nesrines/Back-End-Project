using Newtonsoft.Json;

namespace JuanApp.Controllers;
public class BasketController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    public BasketController(AppDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        List<BasketVM> basketVMs = new();
        string? basket = Request.Cookies["basket"];
        if (!string.IsNullOrWhiteSpace(basket)) basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);

        foreach (BasketVM basketVM in basketVMs)
        {
            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == basketVM.Id);
            basketVM.Title = product.Title;
            basketVM.Image = product.MainImage;
            basketVM.Price = product.Price * (100 - product.Discount) / 100;
        }

        return View(basketVMs);
    }

    public async Task<IActionResult> AddBasket(int? id)
    {
        if (id == null) return BadRequest("Id cannot be null.");
        if (!await _context.Products.AnyAsync(p => !p.IsDeleted && p.Id == id)) return NotFound("Id is incorrect.");

        List<BasketVM> basketVMs = new();
        string? basket = Request.Cookies["basket"];

        if (!string.IsNullOrWhiteSpace(basket))
        {
            basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);
            if (basketVMs.Exists(b => b.Id == id))
            {
                Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
                if (basketVMs.Find(b => b.Id == id).Count < product.Count) basketVMs.Find(b => b.Id == id).Count += 1;
            }
            else basketVMs.Add(new BasketVM { Id = (int)id, Count = 1 });
        }
        else basketVMs.Add(new BasketVM { Id = (int)id, Count = 1 });

        Response.Cookies.Append("basket", JsonConvert.SerializeObject(basketVMs));

        if (User.Identity.IsAuthenticated && User.IsInRole("Member"))
        {
            AppUser? appUser = await _userManager.Users.Include(u => u.BasketProducts.Where(b => !b.IsDeleted))
                .FirstOrDefaultAsync(u =>  u.UserName == User.Identity.Name);

            if (appUser.BasketProducts != null && appUser.BasketProducts.Any())
            {
                BasketProduct userBasketProduct = appUser.BasketProducts.FirstOrDefault(bp => bp.ProductId == id);
                if (userBasketProduct != null) userBasketProduct.Count = basketVMs.FirstOrDefault(b => b.Id == id).Count;
                else
                {
                    userBasketProduct = new()
                    {
                        UserId = appUser.Id,
                        ProductId = id,
                        Count = basketVMs.FirstOrDefault(b => b.Id == id).Count
                    };

                    await _context.BasketProducts.AddAsync(userBasketProduct);
                }
            }
            else
            {
                BasketProduct userBasketProduct = new()
                {
                    UserId = appUser.Id,
                    ProductId = id,
                    Count = basketVMs.FirstOrDefault(b => b.Id == id).Count
                };

                await _context.BasketProducts.AddAsync(userBasketProduct);
            }

            await _context.SaveChangesAsync();
        }

        foreach (BasketVM basketVM in basketVMs)
        {
            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == basketVM.Id);
            basketVM.Title = product.Title;
            basketVM.Image = product.MainImage;
            basketVM.Price = product.Price * (100 - product.Discount) / 100;
        }

        return PartialView("_BasketPartial", basketVMs);
    }

    public async Task<IActionResult> RemoveBasket(int? id)
    {
        if (id == null) return BadRequest("Id cannot be null.");

        List<BasketVM> basketVMs = new();
        string? basket = Request.Cookies["basket"];

        if (!string.IsNullOrWhiteSpace(basket))
        {
            basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);
            if (!basketVMs.Remove(basketVMs.Find(b => b.Id == id))) return NotFound("Id is incorrect.");
        }
        else return BadRequest("Basket does not exist.");

        Response.Cookies.Append("basket", JsonConvert.SerializeObject(basketVMs));

        if (User.Identity.IsAuthenticated && User.IsInRole("Member"))
        {
            AppUser? appUser = await _userManager.Users.Include(u => u.BasketProducts.Where(b => !b.IsDeleted))
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (appUser.BasketProducts != null && appUser.BasketProducts.Any())
            {
                BasketProduct userBasketProduct = appUser.BasketProducts.FirstOrDefault(bp => bp.ProductId == id);
                if (userBasketProduct != null)
                {
                    userBasketProduct.IsDeleted = true;
                    userBasketProduct.DeletedBy = appUser.UserName;
                    userBasketProduct.DeletedDate = DateTime.UtcNow.AddHours(4);

                    await _context.SaveChangesAsync();
                }
            }
        }

        foreach (BasketVM basketVM in basketVMs)
        {
            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == basketVM.Id);
            basketVM.Title = product.Title;
            basketVM.Image = product.MainImage;
            basketVM.Price = product.Price * (100 - product.Discount) / 100;
        }

        return PartialView("_BasketPartial", basketVMs);
    }

    public async Task<IActionResult> CountInc(int? id)
    {
        if (id == null) return BadRequest("Id cannot be null.");

        List<BasketVM> basketVMs = new();
        string? basket = Request.Cookies["basket"];

        if (!string.IsNullOrWhiteSpace(basket))
        {
            basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);
            if (basketVMs.Exists(b => b.Id == id))
            {
                Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
                BasketVM basketVM = basketVMs.Find(b => b.Id == id);
                basketVM.Count = basketVM.Count <= product.Count ? basketVM.Count + 1 : product.Count;
            }
            else return NotFound("Id is incorrect.");
        }
        else return BadRequest("Basket does not exist.");

        Response.Cookies.Append("basket", JsonConvert.SerializeObject(basketVMs));

        if (User.Identity.IsAuthenticated && User.IsInRole("Member"))
        {
            AppUser? appUser = await _userManager.Users.Include(u => u.BasketProducts.Where(b => !b.IsDeleted))
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (appUser.BasketProducts != null && appUser.BasketProducts.Any())
            {
                BasketProduct userBasketProduct = appUser.BasketProducts.FirstOrDefault(bp => bp.ProductId == id);
                if (userBasketProduct != null) userBasketProduct.Count = basketVMs.FirstOrDefault(b => b.Id == id).Count;
                else
                {
                    userBasketProduct = new()
                    {
                        UserId = appUser.Id,
                        ProductId = id,
                        Count = basketVMs.FirstOrDefault(b => b.Id == id).Count
                    };

                    await _context.BasketProducts.AddAsync(userBasketProduct);
                }
            }
            else
            {
                BasketProduct userBasketProduct = new()
                {
                    UserId = appUser.Id,
                    ProductId = id,
                    Count = basketVMs.FirstOrDefault(b => b.Id == id).Count
                };

                await _context.BasketProducts.AddAsync(userBasketProduct);
            }

            await _context.SaveChangesAsync();
        }


        foreach (BasketVM basketVM in basketVMs)
        {
            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == basketVM.Id);
            basketVM.Title = product.Title;
            basketVM.Image = product.MainImage;
            basketVM.Price = product.Price * (100 - product.Discount) / 100;
        }

        return PartialView("_BasketPartial", basketVMs);
    }

    public async Task<IActionResult> CountDec(int? id)
    {
        if (id == null) return BadRequest("Id cannot be null.");

        List<BasketVM> basketVMs = new();
        string? basket = Request.Cookies["basket"];

        if (!string.IsNullOrWhiteSpace(basket))
        {
            basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);
            if (basketVMs.Exists(b => b.Id == id))
            {
                Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
                BasketVM basketVM = basketVMs.Find(b => b.Id == id);
                basketVM.Count = basketVM.Count > 1 ? basketVM.Count - 1 : 1;
            }
            else return NotFound("Id is incorrect.");
        }
        else return BadRequest("Basket does not exist.");

        Response.Cookies.Append("basket", JsonConvert.SerializeObject(basketVMs));

        if (User.Identity.IsAuthenticated && User.IsInRole("Member"))
        {
            AppUser? appUser = await _userManager.Users.Include(u => u.BasketProducts.Where(b => !b.IsDeleted))
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (appUser.BasketProducts != null && appUser.BasketProducts.Any())
            {
                BasketProduct userBasketProduct = appUser.BasketProducts.FirstOrDefault(bp => bp.ProductId == id);
                if (userBasketProduct != null) userBasketProduct.Count = basketVMs.FirstOrDefault(b => b.Id == id).Count;
                else
                {
                    userBasketProduct = new()
                    {
                        UserId = appUser.Id,
                        ProductId = id,
                        Count = basketVMs.FirstOrDefault(b => b.Id == id).Count
                    };

                    await _context.BasketProducts.AddAsync(userBasketProduct);
                }
            }
            else
            {
                BasketProduct userBasketProduct = new()
                {
                    UserId = appUser.Id,
                    ProductId = id,
                    Count = basketVMs.FirstOrDefault(b => b.Id == id).Count
                };

                await _context.BasketProducts.AddAsync(userBasketProduct);
            }

            await _context.SaveChangesAsync();
        }


        foreach (BasketVM basketVM in basketVMs)
        {
            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == basketVM.Id);
            basketVM.Title = product.Title;
            basketVM.Image = product.MainImage;
            basketVM.Price = product.Price * (100 - product.Discount) / 100;
        }

        return PartialView("_BasketPartial", basketVMs);
    }

    public int UpdateCount()
    {
        string? basket = Request.Cookies["basket"];

        if (string.IsNullOrWhiteSpace(basket)) return 0;
        else return JsonConvert.DeserializeObject<List<BasketVM>>(basket).Count();
    }

    public async Task<IActionResult> UpdateCart()
    {
        List<BasketVM> basketVMs = new();
        string? basket = Request.Cookies["basket"];
        if (!string.IsNullOrWhiteSpace(basket)) basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(basket);

        foreach (BasketVM basketVM in basketVMs)
        {
            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == basketVM.Id);
            basketVM.Title = product.Title;
            basketVM.Image = product.MainImage;
            basketVM.Price = product.Price * (100 - product.Discount) / 100;
        }
        return PartialView("_BasketPagePartial", basketVMs);
    }
}