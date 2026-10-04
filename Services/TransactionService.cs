using Microsoft.EntityFrameworkCore;
using SimpleWallet.Data;
using SimpleWallet.Models;

namespace SimpleWallet.Services;

public class TransactionSummary
{
    public decimal TotalReceived { get; set; }
    public decimal TotalSent { get; set; }
    public int Count { get; set; }
}

public interface ITransactionService
{
    Task<Transaction> SendAsync(int senderUserId, string recipientEmail, decimal amount, string? description);

    Task<IReadOnlyList<Transaction>> GetUserTransactionsAsync(int userId, string? filter);

    Task<TransactionSummary> GetSummaryAsync(int userId);
}

public class TransactionService : ITransactionService
{
    private readonly AppDbContext _db;
    private readonly IWalletService _walletService;

    public TransactionService(AppDbContext db, IWalletService walletService)
    {
        _db = db;
        _walletService = walletService;
    }

    public async Task<Transaction> SendAsync(int senderUserId, string recipientEmail, decimal amount, string? description)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Amount must be greater than zero.");

        var sender = await _db.Users.FirstOrDefaultAsync(u => u.Id == senderUserId);
        if (sender is null || !sender.IsActive)
            throw new InvalidOperationException("Your account is not active.");

        var email = (recipientEmail ?? string.Empty).Trim().ToLower();
        var recipient = await _db.Users.Include(u => u.Wallet)
                                       .FirstOrDefaultAsync(u => u.Email == email);

        if (recipient is null || !recipient.IsActive)
            throw new InvalidOperationException("Recipient not found or inactive.");

        if (recipient.Id == senderUserId)
            throw new InvalidOperationException("You cannot send money to yourself.");

        await using var dbTransaction = await _db.Database.BeginTransactionAsync();

        var transferred = await _walletService.TransferAsync(senderUserId, recipient.Id, amount);
        if (!transferred)
        {
            await dbTransaction.RollbackAsync();
            throw new InvalidOperationException("Insufficient balance.");
        }

        var transaction = new Transaction
        {
            SenderUserId = senderUserId,
            RecipientUserId = recipient.Id,
            Amount = amount,
            Description = string.IsNullOrWhiteSpace(description) ? "Transfer" : description.Trim(),
            Type = TransactionTypes.Transfer,
            Status = TransactionStatuses.Completed
        };

        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync();

        await dbTransaction.CommitAsync();

        return transaction;
    }

    public async Task<IReadOnlyList<Transaction>> GetUserTransactionsAsync(int userId, string? filter)
    {
        var query = _db.Transactions
            .Include(t => t.SenderUser)
            .Include(t => t.RecipientUser)
            .Where(t => t.SenderUserId == userId || t.RecipientUserId == userId);

        query = filter?.ToLowerInvariant() switch
        {
            "received" => query.Where(t => t.RecipientUserId == userId &&
                                           (t.Type == TransactionTypes.Deposit || t.SenderUserId != userId)),
            "sent" => query.Where(t => t.SenderUserId == userId &&
                                       (t.Type == TransactionTypes.Withdrawal || t.RecipientUserId != userId)),
            _ => query
        };

        var list = await query.OrderByDescending(t => t.CreatedAt)
                              .ThenByDescending(t => t.Id)
                              .ToListAsync();

        return list;
    }

    public async Task<TransactionSummary> GetSummaryAsync(int userId)
    {
        var amounts = await _db.Transactions
            .Where(t => t.SenderUserId == userId || t.RecipientUserId == userId)
            .Select(t => new { t.Amount, t.Type, t.SenderUserId, t.RecipientUserId })
            .ToListAsync();

        return new TransactionSummary
        {
            TotalReceived = amounts
                .Where(t => t.RecipientUserId == userId &&
                            (t.Type == TransactionTypes.Deposit || t.SenderUserId != userId))
                .Sum(t => t.Amount),
            TotalSent = amounts
                .Where(t => t.SenderUserId == userId &&
                            (t.Type == TransactionTypes.Withdrawal || t.RecipientUserId != userId))
                .Sum(t => t.Amount),
            Count = amounts.Count
        };
    }
}
