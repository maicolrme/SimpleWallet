using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleWallet.Extensions;
using SimpleWallet.Services;
using SimpleWallet.Localization;

namespace SimpleWallet.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;
    private readonly ITextLocalizer _loc;

    public AdminController(IAdminService adminService, ITextLocalizer loc)
    {
        _adminService = adminService;
        _loc = loc;
    }

    [HttpGet("/admin")]
    public async Task<IActionResult> Index()
    {
        return View(await _adminService.GetStatsAsync());
    }

    [HttpGet("/admin/users")]
    public async Task<IActionResult> Users()
    {
        return View(await _adminService.GetUsersAsync());
    }

    [HttpGet("/admin/users/{id:int}")]
    public async Task<IActionResult> UserDetails(int id)
    {
        var user = await _adminService.GetUserAsync(id);
        if (user is null)
            return RedirectToAction(nameof(Users));

        return View(user);
    }

    [HttpPost("/admin/users/{id:int}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int id, bool isActive)
    {
        if (id == User.GetUserId())
        {
            TempData["Error"] = _loc["You cannot change your own status."];
        }
        else
        {
            var ok = await _adminService.SetActiveAsync(id, isActive);
            TempData[ok ? "Success" : "Error"] = ok
                ? (isActive ? _loc["User activated."] : _loc["User deactivated."]) : _loc["User not found."];
        }

        return RedirectToAction(nameof(UserDetails), new { id });
    }

    [HttpPost("/admin/users/{id:int}/role")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRole(int id, string role)
    {
        if (id == User.GetUserId())
        {
            TempData["Error"] = _loc["You cannot change your own role."];
        }
        else
        {
            var ok = await _adminService.SetRoleAsync(id, role);
            TempData[ok ? "Success" : "Error"] = ok ? _loc["Role updated."] : _loc["User not found."];
        }

        return RedirectToAction(nameof(UserDetails), new { id });
    }

    [HttpGet("/admin/wallets")]
    public async Task<IActionResult> Wallets()
    {
        return View(await _adminService.GetWalletsAsync());
    }

    [HttpGet("/admin/transactions")]
    public async Task<IActionResult> Transactions()
    {
        return View(await _adminService.GetTransactionsAsync());
    }
}
