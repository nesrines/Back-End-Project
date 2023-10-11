using JuanApp.Services.Interfaces;
using Newtonsoft.Json;

namespace JuanApp.Services.Implementations;
public class LayoutService : ILayoutService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly UserManager<AppUser> _userManager;
    public LayoutService(AppDbContext context, IHttpContextAccessor contextAccessor, UserManager<AppUser> userManager)
    {
        _context = context;
        _contextAccessor = contextAccessor;
        _userManager = userManager;
    }

    public async Task<List<BasketVM>> GetBasketAsync()
    {
        List<BasketVM> basketVMs = new List<BasketVM>();

        if (_contextAccessor.HttpContext.User.Identity.IsAuthenticated && _contextAccessor.HttpContext.User.IsInRole("Member"))
        {
            AppUser? appUser = await _userManager.Users.Include(u => u.BasketProducts.Where(bp => !bp.IsDeleted))
                .FirstOrDefaultAsync(u => u.NormalizedUserName == _contextAccessor.HttpContext.User.Identity.Name.Trim().ToUpperInvariant());
        
            if (appUser.BasketProducts != null && appUser.BasketProducts.Any())
            {
                foreach (BasketProduct basketProduct in appUser.BasketProducts)
                    basketVMs.Add(new() { Id = (int)basketProduct.ProductId, Count =  basketProduct.Count });
            }
        
        }
        
        else
        {
            string? cookie = _contextAccessor.HttpContext.Request.Cookies["basket"];
            if (!string.IsNullOrWhiteSpace(cookie)) basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(cookie);
        }

        _contextAccessor.HttpContext.Response.Cookies.Append("basket", JsonConvert.SerializeObject(basketVMs));

        foreach(BasketVM basketVM in basketVMs)
        {
            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == basketVM.Id);
            basketVM.Title = product.Title;
            basketVM.Image = product.MainImage;
            basketVM.Price = product.Price * (100 - product.Discount) / 100;
        }

        return basketVMs;
    }

    public async Task<Dictionary<string, string>> GetSettingsAsync()
    {
        return await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);
    }
}