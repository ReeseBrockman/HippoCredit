using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HippoCredit.Data.Models;

public enum TransactionType
{
    Deposit,
    Withdrawal,
    TransferOut,
    TransferIn
}

public class Transaction
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public Account? Account { get; set; }

    [Column(TypeName = "decimal(19,4)")]
    public decimal Amount { get; set; }

    public TransactionType Type { get; set; }

    [Column(TypeName = "decimal(19,4)")]
    public decimal BalanceAfter { get; set; }

    public string Description { get; set; } = string.Empty;

    public int? RelatedAccountId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}