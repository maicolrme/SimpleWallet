using SimpleWallet.Models;
using SimpleWallet.Services;

namespace SimpleWallet.ViewModels;

public class DashboardViewModel
{
    public User Profile { get; set; } = null!;
    public Wallet? Wallet { get; set; }
    public TransactionSummary Summary { get; set; } = new();
    public IReadOnlyList<Transaction> RecentTransactions { get; set; } = Array.Empty<Transaction>();
}
