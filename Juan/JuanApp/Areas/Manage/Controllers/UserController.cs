namespace JuanApp.Areas.Manage.Controllers;
[Area("manage"), Authorize(Roles = "SuperAdmin")]
public class UserController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    public UserController(AppDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    
    public async Task<IActionResult> Index(int currentPage = 1)
    {
        List<AppUser> users = await _userManager.Users.Where(u => u.UserName != User.Identity.Name).ToListAsync();
        foreach (AppUser user in users) user.Roles = await _userManager.GetRolesAsync(user);
        
        return View(PaginatedList<AppUser>.Create(users.AsQueryable(), currentPage, 10, 5));
    }

    public async Task<IActionResult> SetActive(string? id, int currentPage)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();

        AppUser? appUser = await _userManager.FindByIdAsync(id);
        if (appUser == null) return NotFound();

        appUser.IsActive = !appUser.IsActive;
        await _userManager.UpdateAsync(appUser);

        List<AppUser> users = await _userManager.Users.Where(u => u.UserName != User.Identity.Name).ToListAsync();
        foreach (AppUser user in users) user.Roles = await _userManager.GetRolesAsync(user);

        return PartialView("_UsersPartial", PaginatedList<AppUser>.Create(users.AsQueryable(), currentPage, 10, 5));
    }

    public async Task<IActionResult> ResetPassword(string? id, int currentPage)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();

        AppUser? appUser = await _userManager.FindByIdAsync(id);
        if (appUser == null) return NotFound();

        string token = await _userManager.GeneratePasswordResetTokenAsync(appUser);

        await _userManager.ResetPasswordAsync(appUser, token, "Adm!n123");

        List<AppUser> users = await _userManager.Users.Where(u => u.UserName != User.Identity.Name).ToListAsync();
        foreach (AppUser user in users) user.Roles = await _userManager.GetRolesAsync(user);

        return PartialView("_UsersPartial", PaginatedList<AppUser>.Create(users.AsQueryable(), currentPage, 10, 5));
    }
}