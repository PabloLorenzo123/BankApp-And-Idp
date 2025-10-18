using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IDP;
using IDP.Entities.DTOs;
using IDP.Repositories;
using Memento;

public static class Program
{
    private static IServiceProvider _services = new ServiceCollection().BuildServiceProvider();

    public static void Main()
    {
        _services = ConstructDiContainer();
        Console.WriteLine("1) Register an user in the IDP");
        Console.WriteLine("2) Use Memento App");
        int option = int.TryParse(Console.ReadLine(), out var input)? input: throw new Exception("Please provide a number");

        switch (option)
        {
            case 1:
                RegisterUser();
                break;
            case 2:
                Authenticate();
                break;
        }
    }

    private static void RegisterUser() => _services.GetRequiredService<IDPService>().SignUp();
    private static void Authenticate() => _services.GetRequiredService<Authentication>().StartAuthorizationCodeFlow();

    private static IServiceProvider ConstructDiContainer()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("settings.json")
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        // Services.
        services.AddScoped<UsersRepository>();
        services.AddScoped<IDPService>();
        services.AddScoped<OAuthService>();
        services.AddScoped<TokenGenerator>();
        services.AddScoped<Authentication>();

        return services.BuildServiceProvider();
    }
}