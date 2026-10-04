using Microsoft.EntityFrameworkCore;
using SimpleWallet.Models;
using SimpleWallet.Services;

namespace SimpleWallet.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await SeedUserAsync(
            db,
            email: "admin@example.com",
            password: "Admin123!",
            firstName: "Ada",
            lastName: "Admin",
            role: "Admin",
            openingBalance: 0m);

        await SeedUserAsync(
            db,
            email: "user@example.com",
            password: "User123!",
            firstName: "Sam",
            lastName: "User",
            role: "User",
            openingBalance: 1000m);

        if (!await db.Transactions.AnyAsync())
        {
            var user = await db.Users.Include(u => u.Wallet)
                                     .FirstAsync(u => u.Email == "user@example.com");

            db.Transactions.Add(new Transaction
            {
                SenderUserId = user.Id,
                RecipientUserId = user.Id,
                Amount = 1000m,
                Description = "Opening balance",
                Type = TransactionTypes.Deposit,
                Status = TransactionStatuses.Completed,
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            });

            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedUserAsync(
        AppDbContext db,
        string email,
        string password,
        string firstName,
        string lastName,
        string role,
        decimal openingBalance)
    {
        var exists = await db.Users.AnyAsync(u => u.Email == email);
        if (exists)
            return;

        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = PasswordHasher.Hash(password),
            Role = role
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        db.Wallets.Add(new Wallet
        {
            UserId = user.Id,
            Balance = openingBalance,
            Address = $"WLT-{Guid.NewGuid().ToString("N")[..10].ToUpper()}"
        });

        await db.SaveChangesAsync();
    }
}
