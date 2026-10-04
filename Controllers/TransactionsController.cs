using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleWallet.Extensions;
using SimpleWallet.Services;

namespace SimpleWallet.Controllers;

[Authorize]
public class TransactionsController : Controller
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public async Task<IActionResult> Index(string? filter)
    {
        var validFilter = filter?.ToLowerInvariant() is "received" or "sent" ? filter.ToLowerInvariant() : "all";

        var transactions = await _transactionService.GetUserTransactionsAsync(User.GetUserId(), validFilter);

        ViewBag.Filter = validFilter;

        return View(transactions);
    }
}
