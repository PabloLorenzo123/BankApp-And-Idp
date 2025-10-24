using Bank.Entities;
using Data;
using Data.Extensions;
using IDP;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace Bank
{
    public class BankApi(IConfiguration configuration, IDPApi idpApi)
    {
        private readonly string _connectionString = configuration.GetConnectionString("bank") ?? throw new Exception("Bank's connection string not found.");

        public float GetBalance(Account bankAccount)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection.QuerySingleFromFile<BalanceQuery>(Queries.Bank.GetBalance, new { bankAccount.AccountId }).Balance;
        }

        public Account GetBankAccount(string tokenSub)
        {
            var idpUser = idpApi.GetUserByUsername(tokenSub);
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            try
            {
                return connection.QuerySingleFromFile<Account>(Queries.Bank.GetBankAccountByUserId, new { UserId = idpUser.Id });
            } catch
            {
                connection.ExecuteFromFile(Queries.Bank.CreateBankAccount, new { UserId = idpUser.Id });
                return connection.QuerySingleFromFile<Account>(Queries.Bank.GetBankAccountByUserId, new { UserId = idpUser.Id });
            }
        }

        public void DoTransaction(Account sender)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            Account receiver;
            while (true)
            {
                try
                {
                    var sendTo = Utils.PromptNumber("What account (id) will you transfer to?: ");
                    receiver = connection.QuerySingleFromFile<Account>(Queries.Bank.GetAccountById, new { AccountId = sendTo });
                    break;
                }
                catch
                {
                    Console.WriteLine("The account you're trying to transfer to doesn't exist.");
                    continue;
                }
            }

            float balance = connection.QuerySingleFromFile<BalanceQuery>(Queries.Bank.GetBalance, new { AccountId = sender.AccountId }).Balance;
            if (balance <= 0)
            {
                Console.WriteLine($"You can't transfer, you don't have any balance");
                return;
            }

            float amountToTransfer;
            while (true)
            {
                amountToTransfer = Utils.PromptNumber("What amount will you transfer?: ");
                if (amountToTransfer <= 0)
                {
                    Console.WriteLine("You can't transfer a negative amount, please provide something reasonable.");
                    continue;
                }
                if (amountToTransfer > balance)
                {
                    Console.WriteLine($"You can't transfer more than your current balance, the transfer exceeds ${balance - amountToTransfer} your balance");
                    continue;
                }
                break;
            }

            connection.ExecuteFromFile(Queries.Bank.TransferMoney, new
            {
                Amount = amountToTransfer,
                SenderId = sender.AccountId,
                Receiver = receiver.AccountId,
                Date = DateTime.Now.ToString()
            });
        }
    }
}
