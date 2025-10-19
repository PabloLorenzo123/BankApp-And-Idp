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

    private static void RegisterUser() => _services.GetRequiredService<IDPApi>().SignUp();
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
        services.AddScoped<IDPApi>();
        services.AddScoped<OAuthService>();
        services.AddScoped<TokenGenerator>();
        services.AddScoped<Authentication>();

        return services.BuildServiceProvider();
    }
}