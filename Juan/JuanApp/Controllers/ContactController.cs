﻿using JuanApp.DataAccessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JuanApp.Controllers;
public class ContactController : Controller
{
    private readonly AppDbContext _context;
    public ContactController(AppDbContext context) { _context = context; }

    public async Task<IActionResult> Index()
    {
        Dictionary<string, string> settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);
        return View();
    }
}