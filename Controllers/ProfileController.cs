using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleWallet.Extensions;
using SimpleWallet.Services;
using SimpleWallet.Localization;
using SimpleWallet.ViewModels;

namespace SimpleWallet.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IUserService _userService;
    private readonly ITextLocalizer _loc;

    public ProfileController(IUserService userService, ITextLocalizer loc)
    {
        _userService = userService;
        _loc = loc;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userService.GetByIdAsync(User.GetUserId());
        if (user is null)
            return RedirectToAction("Index", "Dashboard");

        var model = new ProfileViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await _userService.UpdateProfileAsync(User.GetUserId(), model.FirstName, model.LastName);

        TempData["Success"] = _loc["Profile updated."];

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var ok = await _userService.ChangePasswordAsync(
            User.GetUserId(),
            model.CurrentPassword,
            model.NewPassword);

        if (!ok)
        {
            ModelState.AddModelError(nameof(model.CurrentPassword), _loc["Current password is incorrect."]);
            return View(model);
        }

        TempData["Success"] = _loc["Password changed."];

        return RedirectToAction(nameof(Index));
    }
}
