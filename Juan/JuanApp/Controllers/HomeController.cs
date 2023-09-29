using Microsoft.AspNetCore.Mvc;

namespace JuanApp.Controllers;
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}