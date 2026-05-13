using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HippoCredit.Data.Models;

public abstract class Account
{
    public int Id { get; set; }

    [Required]
    public string AccountNumber { get; set; } = string.Empty;

    [Required]
    public string OwnerId { get; set; } = string.Empty;

    public ApplicationUser? Owner { get; set; }

    [Column(TypeName = "decimal(19,4)")]
    public decimal Balance { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public abstract string AccountType { get; }

    public abstract decimal CalculateMonthlyInterest();

    public virtual bool CanWithdraw(decimal amount)
    {
        return IsActive && Balance >= amount;
    }
}