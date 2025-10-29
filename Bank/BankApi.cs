using Bank.Entities;
using Data;
using Data.Extensions;
using IDP;

namespace Bank
{
    public class BankApi(ConnectionFactory connectionFactory, IDPApi idpApi)
    {
        public float GetBalance(Account bankAccount)
        {
            using var connection = connectionFactory.CreateConnection(Connections.Bank);
            connection.Open();
            return connection.QuerySingleFromFile<BalanceQuery>(Queries.Bank.GetBalance, new { bankAccount.AccountId }).Balance;
        }

        public Account GetBankAccount(string tokenSub)
        {
            var idpUser = idpApi.GetUserByUsername(tokenSub);
            using var connection = connectionFactory.CreateConnection(Connections.Bank);
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
            using var connection = connectionFactory.CreateConnection(Connections.Bank);
            connection.Open();

            Account receiver;
            while (true)
            {
                try
                {
                    var sendTo = Utils.PromptNumber("What account (id) will you transfer to?: ");
                    receiver = connection.QuerySingleFromFile<Account>(Queries.Bank.GetAccountById, new { AccountId = sendTo });
                    if (receiver.AccountId == sender.AccountId)
                    {
                        Console.WriteLine("Cant' transfer money to yourself.");
                        continue;
                    }
                    break;
                }
                catch
                {
                    Console.WriteLine("The account you're trying to transfer to doesn't exist.");
                    continue;
                }
            }

            long balance = connection.QuerySingleFromFile<BalanceQuery>(Queries.Bank.GetBalance, new { AccountId = sender.AccountId }).Balance;
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
                ReceiverId = receiver.AccountId,
                Date = DateTime.Now.ToString()
            });
        }

        public void SeeLogs()
        {
            using var connection = connectionFactory.CreateConnection(Connections.Bank);
            var logs = connection.QueryFromFile<Log>(Queries.Bank.SeeLogs, new { });
            if (logs.Any())
            {
                foreach (var log in logs)
                {
                    Console.WriteLine(log.Information + " " + log.Date);
                }
            }
            else
            {
                Console.WriteLine("No logs.");
            }
        }
    }
}
