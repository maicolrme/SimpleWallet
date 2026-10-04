using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleWallet.Authentication;
using SimpleWallet.Extensions;
using SimpleWallet.Services;
using SimpleWallet.ViewModels;

namespace SimpleWallet.Controllers.Api;

[ApiController]
[Route("api")]
[Authorize(AuthenticationSchemes = BearerAuthenticationHandler.SchemeName)]
[Produces("application/json")]
public class ApiWalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public ApiWalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    /// <summary>
    /// Wallet balance and address of the current user.
    /// </summary>
    [HttpGet("wallet")]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<WalletDto>> GetWallet()
    {
        var wallet = await _walletService.GetByUserIdAsync(User.GetUserId());

        if (wallet is null)
            return NotFound(new { error = "Wallet not found." });

        return Ok(new WalletDto
        {
            Id = wallet.Id,
            Address = wallet.Address,
            Balance = wallet.Balance,
            Currency = wallet.Currency,
            CreatedAt = wallet.CreatedAt
        });
    }

    /// <summary>
    /// Adds money to the current wallet.
    /// </summary>
    [HttpPost("wallet/deposit")]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<WalletDto>> Deposit(WalletActionRequest request)
    {
        var ok = await _walletService.DepositAsync(
            User.GetUserId(),
            request.Amount,
            string.IsNullOrWhiteSpace(request.Description) ? "Deposit" : request.Description);

        if (!ok)
            return BadRequest(new { error = "Deposit could not be completed." });

        return await GetWallet();
    }

    /// <summary>
    /// Removes money from the current wallet.
    /// </summary>
    [HttpPost("wallet/withdraw")]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<WalletDto>> Withdraw(WalletActionRequest request)
    {
        var ok = await _walletService.WithdrawAsync(
            User.GetUserId(),
            request.Amount,
            string.IsNullOrWhiteSpace(request.Description) ? "Withdrawal" : request.Description);

        if (!ok)
            return BadRequest(new { error = "Insufficient balance." });

        return await GetWallet();
    }
}
