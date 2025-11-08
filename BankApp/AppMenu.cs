using Bank;
using IDP;
using Memento;
using Microsoft.Extensions.Hosting;

namespace BankApp
{
    internal class AppMenu(IDPApi idpApi, BankApi bankApi, Authentication authentication): IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("1) Register an user in the Bank IDP.");
                Console.WriteLine("2) Use Bank App.");
                Console.WriteLine("3) Exit.");
                int option = Utils.PromptNumber("Choose an option: ");

                switch (option)
                {
                    case 1:
                        Console.Clear();
                        idpApi.SignUp();
                        break;
                    case 2:
                        Console.Clear();
                        UseBankApp();
                        break;
                    case 3:
                        return StopAsync(cancellationToken);
                    default:
                        Console.WriteLine("Provide a correct option.");
                        break;
                }
            }
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Thanks for using this app.");
            return Task.CompletedTask;
        }

        private void UseBankApp()
        {
            Console.Clear();

            var identityToken = authentication.DeseralizeToken(authentication.GetIdentityTokenUsingAuthCodeFlow());
            var bankAccount = bankApi.GetBankAccount(identityToken.Payload.Sub);
            Console.WriteLine("Press any key to continue.");
            Console.ReadLine();

            Console.Clear();
            Console.WriteLine($"\nWelcome to the Bank App You're logged in as ID{bankAccount.AccountId}");
            Console.WriteLine("\nBalance: " + bankApi.GetBalance(bankAccount));

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("1) Transfer money.");
                Console.WriteLine("2) See logs.");
                Console.WriteLine("3) Exit.");
                int option = Utils.PromptNumber("Choose an option: ");

                switch (option)
                {
                    case 1:
                        Console.Clear();
                        bankApi.DoTransaction(bankAccount);
                        break;
                    case 2:
                        bankApi.SeeLogs();
                        break;
                    case 3:
                        Console.Clear();
                        return;
                    default:
                        Console.WriteLine("Provide a correct option.");
                        break;
                }
            }
        }
    }
}
