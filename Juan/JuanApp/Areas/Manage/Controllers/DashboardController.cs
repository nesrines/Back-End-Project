using Microsoft.AspNetCore.Mvc;

namespace JuanApp.Areas.Manage.Controllers;
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}