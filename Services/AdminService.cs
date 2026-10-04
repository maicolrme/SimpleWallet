using Microsoft.EntityFrameworkCore;
using SimpleWallet.Data;
using SimpleWallet.Models;

namespace SimpleWallet.Services;

public class AdminStats
{
    public int Users { get; set; }
    public int ActiveUsers { get; set; }
    public int Wallets { get; set; }
    public decimal TotalBalance { get; set; }
    public int Transactions { get; set; }
    public decimal TotalTransferred { get; set; }
}

public interface IAdminService
{
    Task<IReadOnlyList<User>> GetUsersAsync();

    Task<User?> GetUserAsync(int userId);

    Task<bool> SetActiveAsync(int userId, bool isActive);

    Task<bool> SetRoleAsync(int userId, string role);

    Task<IReadOnlyList<Wallet>> GetWalletsAsync();

    Task<IReadOnlyList<Transaction>> GetTransactionsAsync();

    Task<AdminStats> GetStatsAsync();
}

public class AdminService : IAdminService
{
    private readonly AppDbContext _db;

    public AdminService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<User>> GetUsersAsync()
    {
        return await _db.Users.Include(u => u.Wallet)
                              .OrderBy(u => u.Id)
                              .ToListAsync();
    }

    public async Task<User?> GetUserAsync(int userId)
    {
        return await _db.Users.Include(u => u.Wallet)
                              .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<bool> SetActiveAsync(int userId, bool isActive)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null)
            return false;

        user.IsActive = isActive;
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SetRoleAsync(int userId, string role)
    {
        if (role is not ("Admin" or "User"))
            return false;

        var user = await _db.Users.FindAsync(userId);
        if (user is null)
            return false;

        user.Role = role;
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<IReadOnlyList<Wallet>> GetWalletsAsync()
    {
        return await _db.Wallets.Include(w => w.User)
                                .OrderBy(w => w.UserId)
                                .ToListAsync();
    }

    public async Task<IReadOnlyList<Transaction>> GetTransactionsAsync()
    {
        return await _db.Transactions.Include(t => t.SenderUser)
                                     .Include(t => t.RecipientUser)
                                     .OrderByDescending(t => t.CreatedAt)
                                     .ThenByDescending(t => t.Id)
                                     .Take(200)
                                     .ToListAsync();
    }

    public async Task<AdminStats> GetStatsAsync()
    {
        var balances = await _db.Wallets.Select(w => w.Balance).ToListAsync();
        var transferred = await _db.Transactions
            .Where(t => t.Type == TransactionTypes.Transfer)
            .Select(t => t.Amount)
            .ToListAsync();

        return new AdminStats
        {
            Users = await _db.Users.CountAsync(),
            ActiveUsers = await _db.Users.CountAsync(u => u.IsActive),
            Wallets = balances.Count,
            TotalBalance = balances.Sum(),
            Transactions = await _db.Transactions.CountAsync(),
            TotalTransferred = transferred.Sum()
        };
    }
}
