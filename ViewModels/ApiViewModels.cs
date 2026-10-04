using System.ComponentModel.DataAnnotations;

namespace SimpleWallet.ViewModels;

public class TokenRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public class TokenResponse
{
    public string Token { get; set; } = string.Empty;

    public string TokenType { get; set; } = "Bearer";

    public DateTimeOffset ExpiresAt { get; set; }
}

public class CreateTransferRequest
{
    [Required]
    [EmailAddress]
    public string RecipientEmail { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 1_000_000)]
    public decimal Amount { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }
}

public class WalletActionRequest
{
    [Required]
    [Range(0.01, 1_000_000)]
    public decimal Amount { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }
}

public class WalletDto
{
    public int Id { get; set; }

    public string Address { get; set; } = string.Empty;

    public decimal Balance { get; set; }

    public string Currency { get; set; } = "EUR";

    public DateTime CreatedAt { get; set; }
}

public class TransactionDto
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Direction { get; set; } = "sent";

    public string? Counterparty { get; set; }
}

public class UserProfileDto
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = "User";

    public bool IsActive { get; set; }

    public WalletDto? Wallet { get; set; }
}

public class AdminStatsDto
{
    public int Users { get; set; }

    public int ActiveUsers { get; set; }

    public int Wallets { get; set; }

    public decimal TotalBalance { get; set; }

    public int Transactions { get; set; }

    public decimal TotalTransferred { get; set; }
}
