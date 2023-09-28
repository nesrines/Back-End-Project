using Microsoft.AspNetCore.Mvc;

namespace JuanApp.Contollers;
public class ProductController : Controller
{
    public async Task<IActionResult> Index()
    {
        return View();
    }

    public async Task<IActionResult> Details(int? id)
    {
        return View();
    }
}