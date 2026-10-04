using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleWallet.Extensions;
using SimpleWallet.Services;
using SimpleWallet.Localization;
using SimpleWallet.ViewModels;

namespace SimpleWallet.Controllers;

[Authorize]
public class WalletController : Controller
{
    private readonly IWalletService _walletService;
    private readonly ITransactionService _transactionService;
    private readonly ITextLocalizer _loc;

    public WalletController(IWalletService walletService, ITransactionService transactionService, ITextLocalizer loc)
    {
        _walletService = walletService;
        _transactionService = transactionService;
        _loc = loc;
    }

    public async Task<IActionResult> Index()
    {
        var wallet = await _walletService.GetByUserIdAsync(User.GetUserId());
        return View(wallet);
    }

    [HttpGet]
    public IActionResult Send() => View(new SendMoneyViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(SendMoneyViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _transactionService.SendAsync(
                User.GetUserId(),
                model.RecipientEmail,
                model.Amount,
                model.Description);

            TempData["Success"] = _loc["Money sent successfully."];
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, _loc[ex.Message]);
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deposit(WalletActionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = _loc["Enter a valid amount."];
            return RedirectToAction(nameof(Index));
        }

        var ok = await _walletService.DepositAsync(
            User.GetUserId(),
            model.Amount,
            string.IsNullOrWhiteSpace(model.Description) ? "Deposit" : model.Description);

        TempData[ok ? "Success" : "Error"] = ok ? _loc["Deposit completed."] : _loc["Deposit could not be completed."];

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(WalletActionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = _loc["Enter a valid amount."];
            return RedirectToAction(nameof(Index));
        }

        var ok = await _walletService.WithdrawAsync(
            User.GetUserId(),
            model.Amount,
            string.IsNullOrWhiteSpace(model.Description) ? "Withdrawal" : model.Description);

        TempData[ok ? "Success" : "Error"] = ok ? _loc["Withdrawal completed."] : _loc["Insufficient balance."];

        return RedirectToAction(nameof(Index));
    }
}
