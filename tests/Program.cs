using System.Text.Json;
using ValleyInvest;

int assertions = 0;
void Check(bool condition, string name)
{
    assertions++;
    if (!condition) throw new Exception(name);
}

var account = new InvestmentAccount();
int wallet = 10000;
Check(!account.Deposit(-1, ref wallet), "Negative deposit rejected");
Check(!account.Deposit(10001, ref wallet), "Overdraft rejected");
Check(account.Deposit(10000, ref wallet) && wallet == 0 && account.Balance == 10000, "Deposit conserves gold");
for (int day = 1; day <= 28; day++)
    Check(account.ApplyInterest(day, 0.01m), "Interest applies each night");
Check(Math.Abs(account.Balance - 13212.90966898m) < 0.000001m, "28 days of compound interest");
Check(account.TotalInterest == account.Balance - 10000, "Cumulative interest");
decimal balance = account.Balance;
Check(!account.ApplyInterest(28, 0.01m) && account.Balance == balance, "Duplicate day rejected");
Check(!account.ApplyInterest(27, 0.01m), "Older day rejected");
account = JsonSerializer.Deserialize<InvestmentAccount>(JsonSerializer.Serialize(account))!;
Check(account.IsValid && account.Balance == balance && account.LastInterestDay == 28, "Save round trip");
Check(!account.ApplyInterest(28, 0.01m), "Reload cannot double interest");
Check(!account.Withdraw(13213, ref wallet), "Over-withdrawal rejected");
Check(!account.Withdraw(0, ref wallet), "Zero withdrawal rejected");
Check(account.Withdraw(12000, ref wallet) && wallet == 12000, "Partial withdrawal");
Check(account.Withdraw(account.WithdrawalLimit(wallet), ref wallet) && account.Balance < 1, "Withdraw all whole gold");
Check(account.TotalInterest > 3212, "Withdrawal preserves interest history");

account = new InvestmentAccount { Balance = int.MaxValue - 1 };
wallet = 50;
Check(!account.Deposit(2, ref wallet), "Deposit ceiling");
Check(account.Deposit(1, ref wallet), "Deposit up to ceiling");
account.ApplyInterest(1, 0.01m);
Check(account.Balance == int.MaxValue && account.LastInterest == 0, "Interest ceiling");
wallet = int.MaxValue - 5;
Check(account.WithdrawalLimit(wallet) == 5 && !account.Withdraw(6, ref wallet), "Wallet overflow rejected");
Check(account.Withdraw(5, ref wallet) && wallet == int.MaxValue, "Withdrawal up to wallet ceiling");
account = new InvestmentAccount();
account.ApplyInterest(1, 0.01m);
Check(account.Balance == 0, "Empty account");
account.Balance = 100;
account.ApplyInterest(2, 0);
Check(account.Balance == 100, "Zero rate");
account.ApplyInterest(3, 1);
Check(account.Balance == 200, "Maximum rate");
Check(!new InvestmentAccount { Balance = -1 }.IsValid, "Invalid save detected");
Console.WriteLine($"{assertions} assertions passed.");
