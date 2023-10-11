namespace JuanApp.Areas.Manage.Controllers;
[Authorize(Roles = "SuperAdmin")]
public class SettingController : Controller
{
    private readonly AppDbContext _context;
    public SettingController(AppDbContext context) { _context = context; }

    [Authorize(Roles = "Admin, SuperAdmin")]
    public async Task<IActionResult> Index()
    {
        return View(await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value));
    }

    public IActionResult Update(string key)
    {
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string key, Setting setting)
    {
        return RedirectToAction(nameof(Index));
    }
}