using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleWallet.Extensions;
using SimpleWallet.Services;
using SimpleWallet.ViewModels;

namespace SimpleWallet.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IWalletService _walletService;
    private readonly ITransactionService _transactionService;
    private readonly IUserService _userService;

    public DashboardController(
        IWalletService walletService,
        ITransactionService transactionService,
        IUserService userService)
    {
        _walletService = walletService;
        _transactionService = transactionService;
        _userService = userService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();

        var recent = await _transactionService.GetUserTransactionsAsync(userId, "all");

        var model = new DashboardViewModel
        {
            Profile = (await _userService.GetByIdAsync(userId))!,
            Wallet = await _walletService.GetByUserIdAsync(userId),
            Summary = await _transactionService.GetSummaryAsync(userId),
            RecentTransactions = recent.Take(5).ToList()
        };

        return View(model);
    }
}
