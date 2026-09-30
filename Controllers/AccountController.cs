using HotelBooking.Web.Data;
using HotelBooking.Web.Models;
using HotelBooking.Web.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Web.Controllers;

public class AccountController(HotelDbContext dbContext) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var email = model.Email.Trim().ToLowerInvariant();
        var passwordHash = PasswordHasher.Hash(model.Password);
        var account = await dbContext.Accounts.FirstOrDefaultAsync(item => item.Email == email && item.PasswordHash == passwordHash);
        if (account is null)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return View(model);
        }
        HttpContext.Session.SetString("AccountName", account.FullName);
        HttpContext.Session.SetString("AccountEmail", account.Email);
        HttpContext.Session.SetString("AccountRole", account.Role.ToString());
        return LocalRedirect(Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl! : "/");
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null) => View(new RegisterViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        model.Email = model.Email.Trim().ToLowerInvariant();
        if (await dbContext.Accounts.AnyAsync(account => account.Email == model.Email)) ModelState.AddModelError(nameof(model.Email), "Email đã được sử dụng.");
        if (!ModelState.IsValid) return View(model);
        dbContext.Accounts.Add(new Account { FullName = model.FullName, Email = model.Email, Phone = model.Phone, PasswordHash = PasswordHasher.Hash(model.Password) });
        await dbContext.SaveChangesAsync();
        HttpContext.Session.SetString("AccountName", model.FullName);
        HttpContext.Session.SetString("AccountEmail", model.Email);
        HttpContext.Session.SetString("AccountRole", AccountRole.Customer.ToString());
        return LocalRedirect(Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl! : "/");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}