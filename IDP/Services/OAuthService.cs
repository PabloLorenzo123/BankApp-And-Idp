using System.Security.Cryptography;
using System.Text;
using IDP.DTOs;
using Data.IDP.Entities;
using Data.Extensions;
using IDP.Repositories;
using Data;

namespace IDP.Services
{
    public class OAuthService(ConnectionFactory connectionFactory, UsersRepository usersRepository, TokenGenerator tokenGenerator)
    {
        public void RegisterOAuthClient(OAuthClientConfiguration clientConfiguration)
        {
            using var connection = connectionFactory.CreateConnection(Connections.IDP);
            connection.Open();

            try
            {
                connection.QuerySingleFromFile<OAuthClient>(Queries.IDP.GetOAuthClientById, new { clientConfiguration.ClientId });
            }
            catch
            {
                connection.ExecuteFromFile(Queries.IDP.CreateOAuthClient, new { clientConfiguration.ClientId, clientConfiguration.ClientSecret });
            }
        }

        public string ValidateCredentialsAndGetAuthCode(string username, string password, string client_id)
        {
            var user = usersRepository.Get(username);
            var hasher = new HMACSHA512(user.PasswordSalt);

            // Authenticate
            var computedHash = hasher.ComputeHash(Encoding.UTF8.GetBytes(password));
            if (!computedHash.SequenceEqual(user.PasswordHash))
            {
                throw new InvalidOperationException("Could not authenticate user, make sure you're providing the correct username and password");
            }

            // Create authorization code.
            var authorizationCode = Guid.NewGuid().ToString();

            using var connection = connectionFactory.CreateConnection(Connections.IDP);
            connection.Open();
            connection.ExecuteFromFile(Queries.IDP.CreateAuthCode, new { AuthorizationCode = authorizationCode, OAuthClientId = client_id, UserId = user.Id });

            return authorizationCode;
        }

        public string GetSymmetricAuthToken(string authorizationCode, OAuthClientConfiguration oAuthClientConfiguration)
        {
            var authCode = GetAuthCode(authorizationCode, oAuthClientConfiguration);
            var user = usersRepository.Get(authCode.UserId);
            var (_, encodedToken) = tokenGenerator.CreateAuthTokenUsingSymmetricSigning(user, authCode);
            return encodedToken;
        }

        public string GetAsymmetricAuthToken(string authorizationCode, OAuthClientConfiguration oAuthClientConfiguration)
        {
            var authCode = GetAuthCode(authorizationCode, oAuthClientConfiguration);
            var user = usersRepository.Get(authCode.UserId);
            var (_, encodedToken) = tokenGenerator.CreateAuthTokenUsingAsymmetricSigning(user, authCode);
            return encodedToken;
        }

        private AuthorizationCode GetAuthCode(string authorizationCode, OAuthClientConfiguration oAuthClientConfiguration)
        {
            // Validate the authorization code is being used by the right person.
            using var connection = connectionFactory.CreateConnection(Connections.IDP);

            var authCode = connection.QuerySingleFromFile<AuthorizationCode>(Queries.IDP.GetAuthCode, new { Code = authorizationCode });
            var oAuthClient = connection.QuerySingleFromFile<OAuthClient>(Queries.IDP.GetOAuthClientById, new { ClientId = authCode.OAuthClientId });

            // The client id from the api needs to match the client associated with the auth code, and the api's client secret needs to match the registered client secret for the associated oauth client.

            var clientIdMatch = authCode.OAuthClientId == oAuthClientConfiguration.ClientId;            // Validates the auth code is for the requesting user.
            var clientSecretsMatch = oAuthClient.ClientSecret == oAuthClientConfiguration.ClientSecret; // Validates that the client knows the correct secret.

            return clientIdMatch && clientSecretsMatch ? authCode: throw new Exception("The Api's OAuth Credentials don't match the ones associated with the Authorization Code.");
        }
    }
}
