namespace JuanApp.Areas.Manage.Controllers;
[Area("manage"), Authorize(Roles = "Admin, SuperAdmin")]
public class AccountController : Controller
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVM loginVM)
    {
        AppUser? appUser = await _userManager.FindByNameAsync(loginVM.Login) ?? await _userManager.FindByEmailAsync(loginVM.Login);
        
        if(appUser == null)
        {
            ModelState.AddModelError("", "Email (username) or password is incorrect.");
            return View();
        }

        Microsoft.AspNetCore.Identity.SignInResult signInResult = await _signInManager
            .PasswordSignInAsync(appUser, loginVM.Password, loginVM.RememberMe, true);

        if (!signInResult.Succeeded || !appUser.IsActive)
        {
            ModelState.AddModelError("", "Email (username) or password is incorrect.");
            return View();
        }

        int date = appUser.LockoutEnd != null ? (appUser.LockoutEnd - DateTime.UtcNow).Value.Seconds : 0;

        if (date > 0)
        {
            ModelState.AddModelError("", $"Your account is locked out for {date / 60} minutes.");
            return View();
        }

        return RedirectToAction("Index", "Dashboard", new {area = "Manage"});
    }

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    public async Task<IActionResult> Profile()
    {
        AppUser appUser = await _userManager.FindByNameAsync(User.Identity.Name);
        if (appUser == null) return BadRequest();
        ProfileVM profileVM = new()
        {
            Name = appUser.Name,
            Surname = appUser.Surname,
            UserName = appUser.UserName,
            Email = appUser.Email
        };
        return View(profileVM);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileVM profileVM)
    {
        if (!ModelState.IsValid) return View(profileVM);
        AppUser appUser = await _userManager.FindByNameAsync(User.Identity.Name);

        if (appUser.NormalizedUserName != profileVM.UserName.Trim().ToUpperInvariant()) appUser.UserName = profileVM.UserName.Trim();

        if (appUser.NormalizedEmail != profileVM.Email.Trim().ToUpperInvariant()) appUser.Email = profileVM.Email.Trim();

        appUser.Name = profileVM.Name.Trim();
        appUser.Surname = profileVM.Surname.Trim();

        IdentityResult identityResult = await _userManager.UpdateAsync(appUser);
        if (!identityResult.Succeeded)
        {
            foreach (IdentityError identityError in identityResult.Errors) ModelState.AddModelError("", identityError.Description);
            return View(profileVM);
        }

        if (!string.IsNullOrWhiteSpace(profileVM.CurrentPassword))
        {
            if (! await _userManager.CheckPasswordAsync(appUser, profileVM.CurrentPassword))
            {
                ModelState.AddModelError("CurrentPassword", "Password is incorrect.");
                return View(profileVM);
            }
            string token = await _userManager.GeneratePasswordResetTokenAsync(appUser);
            identityResult = await _userManager.ResetPasswordAsync(appUser, token, profileVM.NewPassword);
            if (!identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors) ModelState.AddModelError("", identityError.Description);
                return View(profileVM);
            }
        }

        await _signInManager.SignInAsync(appUser, true);

        return RedirectToAction("Index", "Dashboard", new { area = "Manage" });
    }
}