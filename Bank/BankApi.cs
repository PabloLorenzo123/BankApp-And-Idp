using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bank.Entities;
using Data;
using Data.Extensions;
using IDP;
using IDP.DTOs;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Bank
{
    public class BankApi(IConfiguration configuration, IDPApi idpApi)
    {
        private readonly string _connectionString = configuration.GetConnectionString("bank") ?? throw new Exception("Bank's connection string not found.");
        
        public float GetBalance(Account bankAccount)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection.QuerySingleFromFile<BalanceQuery>(Queries.Bank.GetBalance, new { bankAccount.AccountId} ).Balance;
        }

        public Account GetBankAccount(string tokenSub)
        {
            var idpUser = idpApi.GetUserByUsername(tokenSub);
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            try
            {
                return connection.QuerySingleFromFile<Account>(Queries.Bank.GetBankAccountByUserId, new {UserId = idpUser.Id });
            } catch
            {
                connection.ExecuteFromFile(Queries.Bank.CreateBankAccount, new { UserId = idpUser.Id });
                return connection.QuerySingleFromFile<Account>(Queries.Bank.GetBankAccountByUserId, new { UserId = idpUser.Id });
            }
        }

        public void DoTransaction()
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
        }
    }
}
