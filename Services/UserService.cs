using Microsoft.EntityFrameworkCore;
using SimpleWallet.Data;
using SimpleWallet.Models;

namespace SimpleWallet.Services;

public interface IUserService
{
    Task<User?> GetByIdAsync(int userId);

    Task<User?> GetByEmailAsync(string email);

    Task UpdateProfileAsync(int userId, string firstName, string lastName);

    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
}

public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _db.Users.Include(u => u.Wallet).FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        email = email.Trim().ToLower();
        return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task UpdateProfileAsync(int userId, string firstName, string lastName)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null)
            return;

        user.FirstName = firstName.Trim();
        user.LastName = lastName.Trim();

        await _db.SaveChangesAsync();
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null)
            return false;

        if (!PasswordHasher.Verify(currentPassword, user.PasswordHash))
            return false;

        user.PasswordHash = PasswordHasher.Hash(newPassword);
        await _db.SaveChangesAsync();

        return true;
    }
}
