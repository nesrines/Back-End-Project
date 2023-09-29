﻿using JuanApp.DataAccessLayer;
using JuanApp.Models;
using JuanApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace JuanApp.Services.Implementations;
public class LayoutService : ILayoutService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _contextAccessor;
    public LayoutService(AppDbContext context, IHttpContextAccessor contextAccessor)
    {
        _context = context;
        _contextAccessor = contextAccessor;
    }

    //public async Task<List<BasketVM>> GetBasketAsync()
    //{
    //    List<BasketVM> basketVMs = new List<BasketVM>();
    //    string? cookie = _contextAccessor.HttpContext.Request.Cookies["basket"];
    //    if (!string.IsNullOrWhiteSpace(cookie)) basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(cookie);
    //    foreach (BasketVM basketVM in basketVMs)
    //    {
    //        Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == basketVM.Id);
    //        basketVM.Title = product.Title;
    //        basketVM.Image = product.MainImage;
    //        basketVM.Price = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price;
    //        basketVM.ExTax = product.ExTax;
    //    }
    //    return basketVMs;
    //}

    public async Task<Dictionary<string, string>> GetSettingsAsync()
    {
        return await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);
    }
}