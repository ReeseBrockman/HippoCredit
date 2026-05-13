using System.ComponentModel.DataAnnotations.Schema;

namespace HippoCredit.Data.Models;

public class SavingsAccount : Account
{
    [Column(TypeName = "decimal(5,4)")]
    public decimal InterestRate { get; set; } = 0.0250m;

    public override string AccountType => "Savings";

    public override decimal CalculateMonthlyInterest()
    {
        return Math.Round(Balance * (InterestRate / 12m), 4);
    }
}