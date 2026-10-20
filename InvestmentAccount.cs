namespace ValleyInvest;

/// <summary>Amounts retain fractional gold; transfers always use whole gold.</summary>
public sealed class InvestmentAccount
{
    public decimal Balance { get; set; }
    public decimal TotalInterest { get; set; }
    public decimal LastInterest { get; set; }
    public int LastInterestDay { get; set; } = -1;

    public bool IsValid => Balance >= 0 && Balance <= int.MaxValue
        && TotalInterest >= 0 && LastInterest >= 0 && LastInterestDay >= -1;

    public int DepositLimit(int wallet) => (int)Math.Min(wallet, decimal.Floor(int.MaxValue - Balance));
    public int WithdrawalLimit(int wallet) => (int)Math.Min(decimal.Floor(Balance), int.MaxValue - wallet);

    public bool Deposit(int amount, ref int wallet)
    {
        if (amount <= 0 || amount > DepositLimit(wallet))
            return false;
        wallet -= amount;
        Balance += amount;
        return true;
    }

    public bool Withdraw(int amount, ref int wallet)
    {
        if (amount <= 0 || amount > WithdrawalLimit(wallet))
            return false;
        Balance -= amount;
        wallet += amount;
        return true;
    }

    public bool ApplyInterest(int day, decimal rate)
    {
        if (day <= LastInterestDay)
            return false;
        if (rate < 0 || rate > 1)
            throw new ArgumentOutOfRangeException(nameof(rate));
        LastInterest = Math.Min(decimal.Round(Balance * rate, 8), int.MaxValue - Balance);
        Balance += LastInterest;
        TotalInterest += LastInterest;
        LastInterestDay = day;
        return true;
    }
}
