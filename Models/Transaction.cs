using System.ComponentModel.DataAnnotations;

namespace SimpleWallet.Models;

public static class TransactionTypes
{
    public const string Transfer = "Transfer";
    public const string Deposit = "Deposit";
    public const string Withdrawal = "Withdrawal";
}

public static class TransactionStatuses
{
    public const string Completed = "Completed";
    public const string Failed = "Failed";
}

public class Transaction
{
    public int Id { get; set; }

    public int? SenderUserId { get; set; }

    public User? SenderUser { get; set; }

    public int? RecipientUserId { get; set; }

    public User? RecipientUser { get; set; }

    public decimal Amount { get; set; }

    [MaxLength(255)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Type { get; set; } = TransactionTypes.Transfer;

    [MaxLength(20)]
    public string Status { get; set; } = TransactionStatuses.Completed;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
