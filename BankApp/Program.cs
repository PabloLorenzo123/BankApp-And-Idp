using Bank;
using BankApp;
using Data;
using IDP;
using IDP.Repositories;
using IDP.Services;
using Memento;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public static class Program
{
    public static void Main()
    {
        var builder = new HostBuilder()
            .ConfigureAppConfiguration(configuration =>
            {
                configuration.AddJsonFile("settings.json", optional: false);
            })
            .ConfigureServices(services =>
            {
                services.AddScoped<ConnectionFactory>();
                services.AddScoped<UsersRepository>();
                services.AddScoped<IDPApi>();
                services.AddScoped<BankApi>();
                services.AddScoped<OAuthService>();
                services.AddScoped<TokenGenerator>();
                services.AddScoped<Authentication>();

                services.AddHostedService<AppMenu>();
            });

        var app = builder.Build();
        app.Run();
    }
}