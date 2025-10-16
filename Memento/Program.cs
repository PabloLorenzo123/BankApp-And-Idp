using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IDP;

public static class Program
{
    private static IServiceProvider? _services;

    public static void Main()
    {
        _services = ConstructDiContainer();

        Console.WriteLine("Choose an option: ");
        Console.WriteLine("1) To Sign Up an User.");
        int option = int.TryParse(Console.ReadLine(), out int input) ? input : throw new InvalidOperationException("Invalid input");

        switch (option)
        {
            case 1:
                var idp = _services.GetRequiredService<IDPService>();
                idp.SignUp();
                break;
        }
    }

    private static IServiceProvider ConstructDiContainer()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("settings.json")
            .Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        // Services.
        services.AddScoped<IDP.Repositories.UsersRepository>();
        services.AddScoped<IDPService>();
        return services.BuildServiceProvider();
    }
}