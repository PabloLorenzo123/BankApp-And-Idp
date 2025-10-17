using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IDP;
using IDP.Entities.DTOs;

public static class Program
{
    private static IServiceProvider _services = new ServiceCollection().BuildServiceProvider();

    public static void Main()
    {
        _services = ConstructDiContainer();
        Console.WriteLine("1) Register an user in the IDP");
        Console.WriteLine("2) Start OAuth Demo");
        int option = int.TryParse(Console.ReadLine(), out var input)? input: throw new Exception("Please provide a number");

        switch (option)
        {
            case 1:
                RegisterUser();
                break;
            case 2:
                StartOAuthDemo();
                break;
        }
    }

    private static void RegisterUser() => _services.GetRequiredService<IDPService>().SignUp();
    private static void StartOAuthDemo() => OAuthDemo.Begin();

    /// <summary>
    /// Example of an OAuth Use Case.
    /// </summary>
    private static class OAuthDemo
    {
        private static string _encodedJWTToken = string.Empty;
        private static OAuthClientConfiguration _clientConfiguration = new() { ClientId = "my-api", ClientSecret = "secret", RedirectUri = "https://my-front.com/user-signed-in" };
        private static OAuthService _oAuthService = _services.GetRequiredService<OAuthService>();

        public static void Begin()
        {
            _oAuthService.RegisterOAuthClient(_clientConfiguration); // Register OAuthClient.
            StartAuthorizationCodeFlow();
        }

        /// <summary>
        /// Implements the authorization code flow of OAuth 2.0.
        /// </summary>
        private static void StartAuthorizationCodeFlow()
        {
            Console.WriteLine("Starting Authorization Code Flow...");
            Console.WriteLine(
                string.Format("The client is being redirected to identity-provider.com?client_id={0}&redirect_uri={1}",
                _clientConfiguration.ClientId, _clientConfiguration.RedirectUri)
            );
            Console.WriteLine("You're prompted to enter your credentials");

            Console.WriteLine("Enter username: ");
            string username = Console.ReadLine() ?? throw new InvalidOperationException("Username can't be null");
            Console.WriteLine("Enter password: ");
            string password = Console.ReadLine() ?? throw new InvalidOperationException("Password can't be null");

            var authorizationCode = _oAuthService.ValidateCredentialsAndGetAuthCode(username, password, _clientConfiguration.ClientId);

            Console.WriteLine($"You're being redirected to: {_clientConfiguration.RedirectUri}?authorization_code={authorizationCode}");
            Console.WriteLine("Client (frontend) sends token to API, and API receives it.");
            Console.WriteLine("Api Contacts the IDP server and ask to exchange the authorization code for an access token, using its OAuth Credentials.");

            var accesToken = _oAuthService.GetAuthToken(authorizationCode, _clientConfiguration);
            _encodedJWTToken += accesToken;
            Console.WriteLine("JWT Token: " + accesToken);
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
        services.AddScoped<OAuthService>();

        return services.BuildServiceProvider();
    }
}