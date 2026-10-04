using Microsoft.EntityFrameworkCore;
using SimpleWallet.Data;
using SimpleWallet.Models;

namespace SimpleWallet.Services;

public interface IWalletService
{
    Task<Wallet?> GetByUserIdAsync(int userId);

    Task<Wallet> CreateAsync(int userId);

    Task<bool> DepositAsync(int userId, decimal amount, string description);

    Task<bool> WithdrawAsync(int userId, decimal amount, string description);

    Task<bool> TransferAsync(int senderUserId, int recipientUserId, decimal amount);
}

public class WalletService : IWalletService
{
    private readonly AppDbContext _db;

    public WalletService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Wallet?> GetByUserIdAsync(int userId)
    {
        return await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
    }

    public async Task<Wallet> CreateAsync(int userId)
    {
        var wallet = new Wallet
        {
            UserId = userId,
            Address = $"WLT-{Guid.NewGuid().ToString("N")[..10].ToUpper()}"
        };

        _db.Wallets.Add(wallet);
        await _db.SaveChangesAsync();

        return wallet;
    }

    public async Task<bool> DepositAsync(int userId, decimal amount, string description)
    {
        if (amount <= 0)
            return false;

        var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet is null)
            return false;

        wallet.Balance += amount;

        _db.Transactions.Add(new Transaction
        {
            SenderUserId = userId,
            RecipientUserId = userId,
            Amount = amount,
            Description = description,
            Type = TransactionTypes.Deposit,
            Status = TransactionStatuses.Completed
        });

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> WithdrawAsync(int userId, decimal amount, string description)
    {
        if (amount <= 0)
            return false;

        var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet is null || wallet.Balance < amount)
            return false;

        wallet.Balance -= amount;

        _db.Transactions.Add(new Transaction
        {
            SenderUserId = userId,
            RecipientUserId = userId,
            Amount = amount,
            Description = description,
            Type = TransactionTypes.Withdrawal,
            Status = TransactionStatuses.Completed
        });

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> TransferAsync(int senderUserId, int recipientUserId, decimal amount)
    {
        if (amount <= 0 || senderUserId == recipientUserId)
            return false;

        var senderWallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == senderUserId);
        var recipientWallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == recipientUserId);

        if (senderWallet is null || recipientWallet is null)
            return false;

        if (senderWallet.Balance < amount)
            return false;

        senderWallet.Balance -= amount;
        recipientWallet.Balance += amount;

        await _db.SaveChangesAsync();

        return true;
    }
}
