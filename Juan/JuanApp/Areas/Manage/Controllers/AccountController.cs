using JuanApp.Areas.Manage.ViewModels.AccountVMs;
using JuanApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JuanApp.Areas.Manage.Controllers;
[Area("manage")]
public class AccountController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    //private readonly SignInManager<AppUser> _signInManager;
    public AccountController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterVM registerVM)
    {
        if (!ModelState.IsValid) return View();
        AppUser appUser = new()
        {
            Email = registerVM.Email,
            UserName = registerVM.UserName
        };

        IdentityResult identityResult = await _userManager.CreateAsync(appUser, registerVM.Password);
        if (!identityResult.Succeeded)
        {
            foreach (IdentityError error in identityResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
        return Ok();
    }
}