using HippoCredit.Data;
using HippoCredit.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HippoCredit.Services;

public class AccountService
{
    private readonly ApplicationDbContext _db;

    public AccountService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<Account>> GetAccountsForUserAsync(string userId)
    {
        return await _db.Accounts
            .Where(a => a.OwnerId == userId && a.IsActive)
            .OrderBy(a => a.Id)
            .ToListAsync();
    }

    public async Task<Account?> GetAccountAsync(int accountId, string userId)
    {
        return await _db.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId && a.OwnerId == userId);
    }

    public async Task<List<Transaction>> GetTransactionsAsync(int accountId, string userId, int take = 50)
    {
        var account = await GetAccountAsync(accountId, userId);
        if (account is null) return new List<Transaction>();

        return await _db.Transactions
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.Timestamp)
            .Take(take)
            .ToListAsync();
    }

    public async Task<Account> CreateAccountAsync(string userId, string accountType, decimal openingDeposit)
    {
        if (openingDeposit < 0)
            throw new ArgumentException("Opening deposit cannot be negative.");

        Account account = accountType switch
        {
            "Checking" => new CheckingAccount(),
            "Savings" => new SavingsAccount(),
            _ => throw new ArgumentException($"Unknown account type: {accountType}")
        };

        account.OwnerId = userId;
        account.AccountNumber = GenerateAccountNumber();
        account.Balance = openingDeposit;

        _db.Accounts.Add(account);

        if (openingDeposit > 0)
        {
            _db.Transactions.Add(new Transaction
            {
                Account = account,
                Amount = openingDeposit,
                Type = TransactionType.Deposit,
                BalanceAfter = openingDeposit,
                Description = "Opening deposit"
            });
        }

        await _db.SaveChangesAsync();
        return account;
    }

    private static string GenerateAccountNumber()
    {
        return Random.Shared.NextInt64(1_000_000_000L, 9_999_999_999L).ToString();
    }
}