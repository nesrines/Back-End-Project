using Microsoft.AspNetCore.Mvc;

namespace JuanApp.Areas.Manage.Controllers;
[Area("manage")]
public class AccountController : Controller
{
    public async Task<IActionResult> Register()
    {
        return View();
    }
}