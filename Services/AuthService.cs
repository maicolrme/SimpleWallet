using Microsoft.EntityFrameworkCore;
using SimpleWallet.Data;
using SimpleWallet.Models;

namespace SimpleWallet.Services;

public interface IAuthService
{
    Task<User?> RegisterAsync(string firstName, string lastName, string email, string password);

    Task<User?> ValidateAsync(string email, string password);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IWalletService _walletService;

    public AuthService(AppDbContext db, IWalletService walletService)
    {
        _db = db;
        _walletService = walletService;
    }

    public async Task<User?> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        email = email.Trim().ToLower();

        var exists = await _db.Users.AnyAsync(u => u.Email == email);
        if (exists)
            return null;

        var user = new User
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email,
            PasswordHash = PasswordHasher.Hash(password),
            Role = "User"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await _walletService.CreateAsync(user.Id);

        return user;
    }

    public async Task<User?> ValidateAsync(string email, string password)
    {
        email = email.Trim().ToLower();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || !user.IsActive)
            return null;

        return PasswordHasher.Verify(password, user.PasswordHash) ? user : null;
    }
}
