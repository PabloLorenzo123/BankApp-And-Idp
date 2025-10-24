using Bank;
using IDP;
using IDP.Repositories;
using IDP.Services;
using Memento;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class Program
{
    private static IServiceProvider _services = new ServiceCollection().BuildServiceProvider();
    public static void Main()
    {
        _services = ConstructDiContainer();

        while (true)
        {
            Console.WriteLine("1) Register an user in the Bank IDP.");
            Console.WriteLine("2) Use Bank App.");
            Console.WriteLine("3) Exit.");
            int option = Utils.PromptNumber("Choose an option: ");

            switch (option)
            {
                case 1:
                    RegisterUser();
                    break;
                case 2:
                    UseBankApp();
                    break;
                case 3:
                    return;
                default:
                    Console.WriteLine("Provide a correct option.");
                    break;
            }
        }
    }

    private static void RegisterUser() => _services.GetRequiredService<IDPApi>().SignUp();
    
    private static void UseBankApp()
    {
        var authentication = _services.GetRequiredService<Authentication>();
        var bankApi = _services.GetRequiredService<BankApi>();

        var identityToken = authentication.DeseralizeToken(authentication.GetIdentityTokenUsingAuthCodeFlow());
        var bankAccount = bankApi.GetBankAccount(identityToken.Payload.Sub);

        Console.WriteLine($"\nWelcome to the Bank App You're logged in as {bankAccount.AccountId}");
        Console.WriteLine("\nBalance: " + bankApi.GetBalance(bankAccount));

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("1) Transfer money.");
            int option = Utils.PromptNumber("Choose an option: ");

            switch (option)
            {
                case 1:
                    bankApi.DoTransaction(bankAccount);
                    break;
                default:
                    Console.WriteLine("Provide a correct option.");
                    break;
            }
        }
    }

    private static ServiceProvider ConstructDiContainer()
    {
        var services = new ServiceCollection();

        // Configuration.
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("settings.json")
            .Build();
        services.AddSingleton(configuration);

        // Services.
        services.AddScoped<UsersRepository>();
        services.AddScoped<IDPApi>();
        services.AddScoped<BankApi>();
        services.AddScoped<OAuthService>();
        services.AddScoped<TokenGenerator>();
        services.AddScoped<Authentication>();

        return services.BuildServiceProvider();
    }
}