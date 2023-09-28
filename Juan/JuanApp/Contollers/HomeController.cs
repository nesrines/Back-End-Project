using Microsoft.AspNetCore.Mvc;

namespace JuanApp.Contollers;
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}