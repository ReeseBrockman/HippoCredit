namespace HippoCredit.Data.Models;

public class CheckingAccount : Account
{
    public override string AccountType => "Checking";

    public override decimal CalculateMonthlyInterest()
    {
        return 0m;
    }
}