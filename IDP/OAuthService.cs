using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using IDP.Entities;
using IDP.Entities.DTOs;
using IDP.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IDP
{
    public class OAuthService(IConfiguration configuration, UsersRepository usersRepository, TokenGenerator tokenGenerator)
    {
        private readonly string _connectionString = configuration.GetConnectionString("default") ?? throw new InvalidOperationException("Connection string 'Default' not found.");

        public IEnumerable<(string kid, byte[] key)> GetJWKS() => [
            ("KEY-1", tokenGenerator.publicKey),
         ];

        public void RegisterOAuthClient(OAuthClientConfiguration clientConfiguration)
        {
            using var connectionn = new SqliteConnection(_connectionString);
            connectionn.Open();

            var clientExists = connectionn.Query<OAuthClient>("SELECT * FROM OAUTH_CLIENT WHERE client_id = @ClientId;", new { clientConfiguration.ClientId }).FirstOrDefault();
            if (clientExists == null)
            {
                var command = "INSERT INTO OAUTH_CLIENT (client_id, client_secret) VALUES (@ClientId, @ClientSecret);";
                connectionn.Execute(command, new {clientConfiguration.ClientId, clientConfiguration.ClientSecret});
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

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var cmd = "INSERT INTO AUTHORIZATION_CODE (authorization_code, oauth_client_id, user_id) VALUES (@AuthorizationCode, @OAuthClientId, @UserId);";
            var command = connection.Execute(cmd, new { AuthorizationCode = authorizationCode, OAuthClientId = client_id, UserId = user.Id });

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
            using var connection = new SqliteConnection(_connectionString);
            var authCode = connection.QueryFirstOrDefault<AuthorizationCode>(
                @"SELECT authorization_code as Code,
                  oauth_client_id as OAuthClientId,
                  user_id as UserId,
                  scopes as Scopes
                  FROM AUTHORIZATION_CODE
                  WHERE code = @Code;",
                new { Code = authorizationCode }) ?? throw new Exception("Could not find OAuth Client");

            var oAuthClient = connection.QueryFirstOrDefault<OAuthClient>(
                @"SELECT client_id as ClientId,
                  client_secret as ClientSecret FROM OAUTH_CLIENT WHERE client_id = @OAuthClientId", new { authCode.OAuthClientId }
            ) ?? throw new Exception("Could not retrieve oauth client");

            // The client id from the api needs to match the client associated with the auth code, and the api's client secret needs to match the registered client secret for the associated oauth client.
            if (!(authCode.OAuthClientId == oAuthClientConfiguration.ClientId && oAuthClientConfiguration.ClientSecret == oAuthClient.ClientSecret))
            {
                throw new Exception("The Api's OAuth Credentials don't match the ones associated with the Authorization Code.");
            }

            return authCode;
        }
    }
}
