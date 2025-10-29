using IDP.DTOs;
using Data.IDP.Entities;
using IDP.Repositories;
using IDP.Services;

namespace IDP
{
    /// <summary>
    /// This services represents the IDP Api.
    /// </summary>
    public class IDPApi(UsersRepository usersRepository, OAuthService oAuthService, TokenGenerator tokenGenerator)
    {
        /// <summary>
        /// Register an user
        /// </summary>
        /// <returns></returns>
        public User SignUp()
        {
            Console.WriteLine("Sign Up");
            
            // Prompting
            var username = Utils.PromptText("Enter username: ");
            var password = Utils.PromptText("Enter password: ");

            // Using repository
            var user = usersRepository.Create(new CreateUserDto { Username = username, Password = password });
            Console.WriteLine($"User {user.Username} created with ID {user.Id}");
            return user;
        }

        /// <summary>
        /// Retrieve the public keys to validate JWT tokens emitted by this IDP.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<(string kid, byte[] key)> GetJWKS() => [
            ("KEY-1", tokenGenerator.publicKey),
         ];

        public void RegisterOAuthClient(OAuthClientConfiguration configuration) => oAuthService.RegisterOAuthClient(configuration);

        public string ValidateCredentialsAndGetAuthCode(string username, string password, string client_id, string scopes) => oAuthService.ValidateCredentialsAndGetAuthCode(username, password, client_id, scopes);

        public string GetAsymmetricAuthToken(string authorizationCode, OAuthClientConfiguration client) => oAuthService.GetAsymmetricAuthToken(authorizationCode, client);

        public User GetUserByUsername(string username) => usersRepository.Get(username);
    }
}
