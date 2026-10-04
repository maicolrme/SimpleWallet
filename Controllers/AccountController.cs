using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SimpleWallet.Extensions;
using SimpleWallet.Services;
using SimpleWallet.Localization;
using SimpleWallet.ViewModels;

namespace SimpleWallet.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly ITextLocalizer _loc;

    public AccountController(IAuthService authService, ITextLocalizer loc)
    {
        _authService = authService;
        _loc = loc;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectHome(User.IsInRole("Admin"));

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var user = await _authService.ValidateAsync(model.Email, model.Password);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, _loc["Invalid email or password."]);
            return View(model);
        }

        await CookieAuth.SignInAsync(HttpContext, user);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectHome(user.Role == "Admin");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectHome(User.IsInRole("Admin"));

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _authService.RegisterAsync(
            model.FirstName,
            model.LastName,
            model.Email,
            model.Password);

        if (user is null)
        {
            ModelState.AddModelError(nameof(model.Email), _loc["An account with this email already exists."]);
            return View(model);
        }

        await CookieAuth.SignInAsync(HttpContext, user);

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();

        return RedirectToAction("Index", "Home");
    }

    private IActionResult RedirectHome(bool isAdmin) =>
        isAdmin
            ? RedirectToAction("Index", "Admin")
            : RedirectToAction("Index", "Dashboard");
}
