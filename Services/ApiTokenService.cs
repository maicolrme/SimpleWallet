using Microsoft.AspNetCore.DataProtection;

namespace SimpleWallet.Services;

public interface IApiTokenService
{
    string IssueToken(int userId, TimeSpan lifetime);

    int? ValidateToken(string token);
}

/// <summary>
/// Stateless bearer tokens protected with ASP.NET Core Data Protection.
/// No token table is needed: the payload carries the user id and the expiry.
/// </summary>
public class ApiTokenService : IApiTokenService
{
    private const string Purpose = "SimpleWallet.ApiToken.v1";
    private readonly IDataProtector _protector;

    public ApiTokenService(IDataProtectionProvider dataProtection)
    {
        _protector = dataProtection.CreateProtector(Purpose);
    }

    public string IssueToken(int userId, TimeSpan lifetime)
    {
        var expiresAt = DateTimeOffset.UtcNow.Add(lifetime).ToUnixTimeSeconds();
        return _protector.Protect($"{userId}|{expiresAt}");
    }

    public int? ValidateToken(string token)
    {
        string payload;

        try
        {
            payload = _protector.Unprotect(token);
        }
        catch (Exception)
        {
            return null;
        }

        var parts = payload.Split('|');
        if (parts.Length != 2)
            return null;

        if (!int.TryParse(parts[0], out var userId))
            return null;

        if (!long.TryParse(parts[1], out var expiresAt))
            return null;

        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= expiresAt)
            return null;

        return userId;
    }
}
