using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleWallet.Authentication;
using SimpleWallet.Extensions;
using SimpleWallet.Models;
using SimpleWallet.Services;
using SimpleWallet.ViewModels;

namespace SimpleWallet.Controllers.Api;

[ApiController]
[Route("api")]
[Authorize(AuthenticationSchemes = BearerAuthenticationHandler.SchemeName)]
[Produces("application/json")]
public class ApiTransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public ApiTransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    /// <summary>
    /// Transaction history of the current user. Filter: all, received or sent.
    /// </summary>
    [HttpGet("transactions")]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TransactionDto>>> List([FromQuery] string? filter)
    {
        var userId = User.GetUserId();
        var validFilter = filter?.ToLowerInvariant() is "received" or "sent" ? filter.ToLowerInvariant() : "all";

        var transactions = await _transactionService.GetUserTransactionsAsync(userId, validFilter);

        return Ok(transactions.Select(t => ToDto(t, userId)).ToList());
    }

    /// <summary>
    /// Sends money to another user by email.
    /// </summary>
    [HttpPost("transfers")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Send(CreateTransferRequest request)
    {
        var userId = User.GetUserId();

        try
        {
            var transaction = await _transactionService.SendAsync(
                userId,
                request.RecipientEmail,
                request.Amount,
                request.Description);

            return Ok(ToDto(transaction, userId));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    internal static TransactionDto ToDto(Transaction transaction, int viewerId)
    {
        var received = transaction.RecipientUserId == viewerId &&
                       (transaction.Type == TransactionTypes.Deposit || transaction.SenderUserId != viewerId);

        var isSelfService = transaction.Type is TransactionTypes.Deposit or TransactionTypes.Withdrawal;

        var counterparty = isSelfService
            ? null
            : received
                ? transaction.SenderUser?.FullName
                : transaction.RecipientUser?.FullName;

        return new TransactionDto
        {
            Id = transaction.Id,
            CreatedAt = transaction.CreatedAt,
            Type = transaction.Type,
            Status = transaction.Status,
            Amount = transaction.Amount,
            Description = transaction.Description,
            Direction = received ? "received" : "sent",
            Counterparty = counterparty
        };
    }
}
