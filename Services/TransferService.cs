using HippoCredit.Data;
using HippoCredit.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HippoCredit.Services;

public class TransferService
{
    private readonly ApplicationDbContext _db;

    public TransferService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<TransferResult> TransferAsync(string userId, int fromAccountId, int toAccountId, decimal amount, string memo)
    {
        if (amount <= 0)
            return TransferResult.Fail("Amount must be greater than zero.");

        if (fromAccountId == toAccountId)
            return TransferResult.Fail("Cannot transfer to the same account.");

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var fromAccount = await _db.Accounts
                .FirstOrDefaultAsync(a => a.Id == fromAccountId && a.OwnerId == userId);

            var toAccount = await _db.Accounts
                .FirstOrDefaultAsync(a => a.Id == toAccountId && a.OwnerId == userId);

            if (fromAccount is null || toAccount is null)
                return TransferResult.Fail("Account not found.");

            if (!fromAccount.CanWithdraw(amount))
                return TransferResult.Fail("Insufficient funds.");

            fromAccount.Balance -= amount;
            toAccount.Balance += amount;

            _db.Transactions.Add(new Transaction
            {
                AccountId = fromAccount.Id,
                Amount = amount,
                Type = TransactionType.TransferOut,
                BalanceAfter = fromAccount.Balance,
                Description = $"Transfer to {toAccount.AccountNumber}. {memo}",
                RelatedAccountId = toAccount.Id
            });

            _db.Transactions.Add(new Transaction
            {
                AccountId = toAccount.Id,
                Amount = amount,
                Type = TransactionType.TransferIn,
                BalanceAfter = toAccount.Balance,
                Description = $"Transfer from {fromAccount.AccountNumber}. {memo}",
                RelatedAccountId = fromAccount.Id
            });

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return TransferResult.Ok();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return TransferResult.Fail($"Transfer failed: {ex.Message}");
        }
    }
}

public record TransferResult(bool Success, string? ErrorMessage)
{
    public static TransferResult Ok() => new(true, null);
    public static TransferResult Fail(string error) => new(false, error);
}