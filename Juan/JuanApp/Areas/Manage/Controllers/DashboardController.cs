namespace JuanApp.Areas.Manage.Controllers;
[Area("manage"), Authorize(Roles = "Admin, SuperAdmin")]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}