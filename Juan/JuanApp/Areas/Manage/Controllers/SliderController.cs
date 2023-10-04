using Microsoft.AspNetCore.Mvc;

namespace JuanApp.Areas.Manage.Controllers;
[Area("manage")]
public class SliderController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}