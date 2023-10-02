﻿using JuanApp.DataAccessLayer;
using JuanApp.Models;
using JuanApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JuanApp.Controllers;
public class ProductController : Controller
{
    private readonly AppDbContext _context;
    public ProductController(AppDbContext context) { _context = context; }

    public IActionResult Index(int currentPage = 1)
    {
        IQueryable<Product> products = _context.Products
            .Where(p => !p.IsDeleted && p.Count > 0)
            .OrderByDescending(p => p.Id);

        return View(PaginatedList<Product>.Create(products, currentPage, 6, 4));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return BadRequest();
        Product? product = await _context.Products.FirstOrDefaultAsync(p => !p.IsDeleted && p.Id == id);
        if (product == null) return NotFound();
        return View(product);
    }

    public async Task<IActionResult> LoadMore(int? pageIndex)
    {
        if (pageIndex == null || pageIndex <= 1) return BadRequest();

        IQueryable<Product> products = _context.Products
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.Id);

        int MaxPage = (int)Math.Ceiling((decimal)products.Count() / 6);

        if (pageIndex > MaxPage) return BadRequest();

        products = products.Skip((int)(pageIndex - 1) * 6).Take(6);
        return PartialView("_LoadMorePartial", products);
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