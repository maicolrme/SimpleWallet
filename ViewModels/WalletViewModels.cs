using System.ComponentModel.DataAnnotations;

namespace SimpleWallet.ViewModels;

public class SendMoneyViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Recipient email")]
    public string RecipientEmail { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 1_000_000, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }
}

public class WalletActionViewModel
{
    [Required]
    [Range(0.01, 1_000_000, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }
}
