namespace JuanApp.Areas.Manage.Controllers;
[Area("manage"), Authorize(Roles = "Admin, SuperAdmin")]
public class SliderController : Controller
{
    private readonly AppDbContext _context;
    private IWebHostEnvironment _env;
    public SliderController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Sliders.Where(s => !s.IsDeleted).ToListAsync());
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Slider slider)
    {
        if (!ModelState.IsValid) return View(slider);

        slider.BackgroundImage = await slider.BackgroundFile.SaveAsync(_env.WebRootPath, "assets", "img", "slider");

        await _context.Sliders.AddAsync(slider);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}