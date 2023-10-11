using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Newtonsoft.Json;

namespace JuanApp.Controllers;
public class AccountController : Controller
{
    private readonly IWebHostEnvironment _env;
    private readonly AppDbContext _context;
    private readonly SmtpSetting _smtpSetting;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    public AccountController(IWebHostEnvironment env, IOptions<SmtpSetting> options, AppDbContext context,
           UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    {
        _env = env;
        _context = context;
        _smtpSetting = options.Value;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public IActionResult Register() { return View(); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVM registerVM)
    {
        if (!ModelState.IsValid) return View();
        AppUser appUser = new()
        {
            Name = registerVM.Name,
            Surname = registerVM.Surname,
            UserName = registerVM.UserName,
            Email = registerVM.Email,
            IsActive = true
        };

        IdentityResult identityResult = await _userManager.CreateAsync(appUser, registerVM.Password);
        if (!identityResult.Succeeded)
        {
            foreach (IdentityError error in identityResult.Errors) ModelState.AddModelError("", error.Description);
            return View(registerVM);
        }

        await _userManager.AddToRoleAsync(appUser, "Member");

        string templateFullPath = Path.Combine(_env.WebRootPath, "templates", "EmailConfirm.html");
        string templateContent = await System.IO.File.ReadAllTextAsync(templateFullPath);
        
        string token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
        string? url = Url.Action("EmailConfirm", "Account", new { id = appUser.Id, token },
            HttpContext.Request.Scheme, HttpContext.Request.Host.ToString());

        MimeMessage mimeMessage = new MimeMessage();
        mimeMessage.From.Add(MailboxAddress.Parse(_smtpSetting.Email));
        mimeMessage.To.Add(MailboxAddress.Parse(appUser.Email));
        mimeMessage.Subject = "Email Confirmation";
        mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = templateContent.Replace("{{url}}", url) };

        using (SmtpClient client = new())
        {
            await client.ConnectAsync(_smtpSetting.Host, _smtpSetting.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtpSetting.Email, _smtpSetting.Password);
            await client.SendAsync(mimeMessage);
            await client.DisconnectAsync(true);
        }

        await _signInManager.SignInAsync(appUser, false);

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> EmailConfirm(string id, string token)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();

        AppUser? appUser = await _userManager.FindByIdAsync(id);

        if (appUser == null) return NotFound();

        if (!appUser.IsActive) return BadRequest();

        if (appUser.EmailConfirmed) return Conflict();

        IdentityResult identityResult = await _userManager.ConfirmEmailAsync(appUser, token);
        if (!identityResult.Succeeded)
        {
            foreach (IdentityError identityError in identityResult.Errors) ModelState.AddModelError("", identityError.Description);
            return View(nameof(Login));
        }

        await _signInManager.SignInAsync(appUser, true);

        return RedirectToAction("Index", "Home");
    }

    public IActionResult Login() { return View(); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVM loginVM)
    {
        AppUser? appUser = await _userManager.Users.Include(u => u.BasketProducts.Where(bp => !bp.IsDeleted))
            .FirstOrDefaultAsync(u => u.NormalizedEmail == loginVM.Login.Trim().ToUpperInvariant() ||
                                    u.NormalizedUserName ==loginVM.Login.Trim().ToUpperInvariant() );

        if (appUser == null)
        {
            ModelState.AddModelError("", "Email (username) or password is incorrect.");
            return View();
        }
        
        IList<string> roles = await _userManager.GetRolesAsync(appUser);
        if (!roles.Any(s => s == "Member"))
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

        if (appUser.BasketProducts != null && appUser.BasketProducts.Any())
        {
            List<BasketVM> basketVMs = new();
            foreach (BasketProduct basketProduct in appUser.BasketProducts)
                basketVMs.Add(new() { Id = (int)basketProduct.ProductId, Count = basketProduct.Count });

            Response.Cookies.Append("basket", JsonConvert.SerializeObject(basketVMs));
        }

        return RedirectToAction("Index", "Home");
    }

    [Authorize(Roles = "Member")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult ForgotPassword() { return View(); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordVM forgotPasswordVM)
    {
        if (!ModelState.IsValid) return View(forgotPasswordVM);
        
        AppUser? appUser = await _userManager.FindByEmailAsync(forgotPasswordVM.Email);
        
        if (appUser == null || !appUser.IsActive)
        {
            ModelState.AddModelError("Email", "Email is incorrect.");
            return View(forgotPasswordVM);
        }

        string token = await _userManager.GeneratePasswordResetTokenAsync(appUser);
        string templateFullPath = Path.Combine(_env.WebRootPath, "templates", "ResetPassword.html");
        string templateContent = await System.IO.File.ReadAllTextAsync(templateFullPath);
        string? url = Url.Action("ResetPassword", "Account", new { id = appUser.Id, token}, Request.Scheme, Request.Host.ToString());

        templateContent = templateContent.Replace("{{action_url}}", url).Replace("{{name}}", appUser.Name);

        MimeMessage mimeMessage = new();
        mimeMessage.From.Add(MailboxAddress.Parse(_smtpSetting.Email));
        mimeMessage.To.Add(MailboxAddress.Parse(appUser.Email));
        mimeMessage.Subject = "Reset Password";
        mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = templateContent };

        using (SmtpClient client = new())
        {
            await client.ConnectAsync(_smtpSetting.Host, _smtpSetting.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtpSetting.Email, _smtpSetting.Password);
            await client.SendAsync(mimeMessage);
            await client.DisconnectAsync(true);
        }
        ViewBag.EmailSent = true;
        return View();
    }

    public ActionResult ResetPassword() { return View(); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string id, string token, ResetPasswordVM resetPasswordVM)
    {
        if (!ModelState.IsValid) return View(resetPasswordVM);

        if (string.IsNullOrWhiteSpace(id))
        {
            ModelState.AddModelError("", "Id is incorrect");
            return View(resetPasswordVM);
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            ModelState.AddModelError("", "Token is incorrect");
            return View(resetPasswordVM);
        }

        AppUser? appUser = await _userManager.FindByIdAsync(id);
        if (appUser == null)
        {
            ModelState.AddModelError("", "Id is incorrect");
            return View(resetPasswordVM);
        }

        IdentityResult identityResult = await _userManager.ResetPasswordAsync(appUser, token, resetPasswordVM.Password);

        if (!identityResult.Succeeded)
        {
            foreach (IdentityError identityError in identityResult.Errors) ModelState.AddModelError("", identityError.Description);
            return View(resetPasswordVM);
        }

        return RedirectToAction(nameof(Login));
    }

    [Authorize(Roles = "Member")]
    public async Task<IActionResult> Profile()
    {
        AppUser appUser = await _userManager.Users.Include(u => u.Addresses.Where(a => !a.IsDeleted))
            .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

        ProfilePageVM profilePageVM = new()
        {
            Address = new(),
            Addresses = appUser.Addresses,
            ProfileVM = new()
            {
                Name = appUser.Name,
                Surname = appUser.Surname,
                UserName = appUser.UserName,
                Email = appUser.Email
            }
        };

        return View(profilePageVM);
    }

    [HttpPost, Authorize(Roles = "Member"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ProfileAccount(ProfileVM profileVM)
    {
        AppUser? appUser = await _userManager.Users.Include(u => u.Addresses.Where(a => !a.IsDeleted))
            .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

        ProfilePageVM profilePageVM = new() {
            Address = new(),
            Addresses = appUser.Addresses,
            ProfileVM = profileVM
        };

        if (!ModelState.IsValid) return View("Profile", profilePageVM);

        if (appUser.NormalizedUserName != profileVM.UserName.Trim().ToUpperInvariant()) appUser.UserName = profileVM.UserName.Trim();

        if (appUser.NormalizedEmail != profileVM.Email.Trim().ToUpperInvariant()) appUser.Email = profileVM.Email.Trim();

        appUser.Name = profileVM.Name.Trim();
        appUser.Surname = profileVM.Surname.Trim();

        IdentityResult identityResult = await _userManager.UpdateAsync(appUser);
        if (!identityResult.Succeeded)
        {
            foreach (IdentityError identityError in identityResult.Errors) ModelState.AddModelError("", identityError.Description);
            return View(profilePageVM);
        }

        if (!string.IsNullOrWhiteSpace(profileVM.CurrentPassword))
        {
            if (!await _userManager.CheckPasswordAsync(appUser, profileVM.CurrentPassword))
            {
                ModelState.AddModelError("CurrentPassword", "Password is incorrect.");
                return View(profileVM);
            }
            string token = await _userManager.GeneratePasswordResetTokenAsync(appUser);
            identityResult = await _userManager.ResetPasswordAsync(appUser, token, profileVM.NewPassword);
            if (!identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors) ModelState.AddModelError("", identityError.Description);
                return View(profilePageVM);
            }
        }

        await _signInManager.SignInAsync(appUser, true);

        return RedirectToAction(nameof(Profile));
    }

    [HttpPost, Authorize(Roles = "Member"), ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAddress(Address address)
    {
        TempData["Tab"] = "Address";

        AppUser? appUser = await _userManager.Users.Include(u => u.Addresses.Where(a => !a.IsDeleted))
            .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
        
        ProfilePageVM profilePageVM = new()
        {
            Address = address,
            Addresses = appUser.Addresses,
            ProfileVM = new()
            {
                Name = appUser.Name,
                Surname = appUser.Surname,
                UserName = appUser.UserName,
                Email = appUser.Email
            }
        };

        if (!ModelState.IsValid)
        {
            TempData["AddressForm"] = "add";
            return View("Profile", profilePageVM);
        }

        if (string.IsNullOrWhiteSpace(address.Country))
        {
            TempData["AddressForm"] = "add";
            ModelState.AddModelError("Country", "Required");
            return View("Profile", profilePageVM);
        }

        if (string.IsNullOrWhiteSpace(address.TownCity))
        {
            TempData["AddressForm"] = "add";
            ModelState.AddModelError("TownCity", "Required");
            return View("Profile", profilePageVM);
        }

        if (string.IsNullOrWhiteSpace(address.Line1))
        {
            TempData["AddressForm"] = "add";
            ModelState.AddModelError("Line1", "Required");
            return View("Profile", profilePageVM);
        }

        if (string.IsNullOrWhiteSpace(address.Line2))
        {
            TempData["AddressForm"] = "add";
            ModelState.AddModelError("Line2", "Required");
            return View("Profile", profilePageVM);
        }

        if (string.IsNullOrWhiteSpace(address.ZipCode))
        {
            TempData["AddressForm"] = "add";
            ModelState.AddModelError("ZipCode", "Required");
            return View("Profile", profilePageVM);
        }

        if (address.IsDefault && appUser.Addresses != null && appUser.Addresses.Count() > 0)
                appUser.Addresses.FirstOrDefault(a => a.IsDefault).IsDefault = false;

        else if (appUser.Addresses == null || appUser.Addresses.Count() < 1) address.IsDefault = true;

        address.UserId = appUser.Id;
        address.CreatedBy = appUser.UserName;

        await _context.Addresses.AddAsync(address);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Profile));
    }

    [Authorize(Roles = "Member")]
    public async Task<IActionResult> EditAddress (int? id)
    {
        TempData["Tab"] = "Address";

        if (id == null) return BadRequest();

        AppUser? appUser = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

        Address? address = await _context.Addresses
            .FirstOrDefaultAsync(a => !a.IsDeleted && a.UserId == appUser.Id && a.Id == id);

        if (address == null) return NotFound();

        return PartialView("_EditAddressPartial", address);
    }

    [HttpPost, Authorize(Roles = "Member"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAddress(Address address)
    {
        TempData["Tab"] = "Address";

        AppUser? appUser = await _userManager.Users.Include(u => u.Addresses.Where(a => !a.IsDeleted))
            .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
        
        ProfilePageVM profilePageVM = new()
        {
            Address = address,
            Addresses = appUser.Addresses,
            ProfileVM = new()
            {
                Name = appUser.Name,
                Surname = appUser.Surname,
                UserName = appUser.UserName,
                Email = appUser.Email
            }
        };

        if (!ModelState.IsValid)
        {
            TempData["AddressForm"] = "edit";
            return View("Profile", profilePageVM);
        }

        Address? dbAddress = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == address.Id);

        if (dbAddress == null) return NotFound();

        if (string.IsNullOrWhiteSpace(address.Country))
        {
            TempData["AddressForm"] = "edit";
            ModelState.AddModelError("Country", "Required");
            return View("Profile", profilePageVM);
        }

        if (string.IsNullOrWhiteSpace(address.TownCity))
        {
            TempData["AddressForm"] = "edit";
            ModelState.AddModelError("TownCity", "Required");
            return View("Profile", profilePageVM);
        }

        if (string.IsNullOrWhiteSpace(address.Line1))
        {
            TempData["AddressForm"] = "edit";
            ModelState.AddModelError("Line1", "Required");
            return View("Profile", profilePageVM);
        }

        if (string.IsNullOrWhiteSpace(address.Line2))
        {
            TempData["AddressForm"] = "edit";
            ModelState.AddModelError("Line2", "Required");
            return View("Profile", profilePageVM);
        }

        if (string.IsNullOrWhiteSpace(address.ZipCode))
        {
            TempData["AddressForm"] = "edit";
            ModelState.AddModelError("ZipCode", "Required");
            return View("Profile", profilePageVM);
        }

        if (address.IsDefault != dbAddress.IsDefault && appUser.Addresses != null && appUser.Addresses.Any())
        {
            if (address.IsDefault) appUser.Addresses.FirstOrDefault(a => a.IsDefault).IsDefault = false;
            else appUser.Addresses.FirstOrDefault().IsDefault = true;
            
            dbAddress.IsDefault = address.IsDefault;
        }

        dbAddress.Country = address.Country;
        dbAddress.TownCity = address.TownCity;
        dbAddress.Line1 = address.Line1;
        dbAddress.Line2 = address.Line2;
        dbAddress.ZipCode = address.ZipCode;
        dbAddress.UpdatedBy = appUser.UserName;
        dbAddress.UpdatedDate = DateTime.UtcNow.AddHours(4);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Profile));
    }
}