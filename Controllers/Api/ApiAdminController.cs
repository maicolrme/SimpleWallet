using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleWallet.Authentication;
using SimpleWallet.Services;
using SimpleWallet.ViewModels;

namespace SimpleWallet.Controllers.Api;

[ApiController]
[Route("api/admin")]
[Authorize(AuthenticationSchemes = BearerAuthenticationHandler.SchemeName, Roles = "Admin")]
[Produces("application/json")]
public class ApiAdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public ApiAdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Platform statistics. Admin only.
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(AdminStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminStatsDto>> Stats()
    {
        var stats = await _adminService.GetStatsAsync();

        return Ok(new AdminStatsDto
        {
            Users = stats.Users,
            ActiveUsers = stats.ActiveUsers,
            Wallets = stats.Wallets,
            TotalBalance = stats.TotalBalance,
            Transactions = stats.Transactions,
            TotalTransferred = stats.TotalTransferred
        });
    }

    /// <summary>
    /// All users with wallets. Admin only.
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(IReadOnlyList<UserProfileDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserProfileDto>>> Users()
    {
        var users = await _adminService.GetUsersAsync();

        return Ok(users.Select(ApiTokenController.ToDto).ToList());
    }
}
