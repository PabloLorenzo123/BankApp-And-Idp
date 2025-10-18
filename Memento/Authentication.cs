using IDP.Entities.DTOs;
using IDP.Services;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Memento
{
    public class Authentication
    {
        public string _encodedJWTToken = string.Empty;
        private readonly OAuthClientConfiguration _clientConfiguration = new()
        {
            ClientId = "my-api",
            ClientSecret = "secret",
            RedirectUri = "https://my-front.com/user-signed-in"
        };
        private readonly OAuthService _oAuthService;

        public Authentication(OAuthService oAuthService)
        {
            _oAuthService = oAuthService;
            _oAuthService.RegisterOAuthClient(_clientConfiguration); // Register OAuthClient.
            StartAuthorizationCodeFlow();
        }

        /// <summary>
        /// Implements the authorization code flow of OAuth 2.0.
        /// </summary>
        public void StartAuthorizationCodeFlow()
        {
            var oAuthFlow = "authorization_code";
            var scopes = string.Join(";", ["fullaccess"]);

            Console.WriteLine("Starting Authorization Code Flow...");
            Console.WriteLine("The client is being redirected to: ");
            Console.WriteLine(
                string.Format("\nidentity-provider.com?client_id={0}&redirect_uri={1}&code={2}code&scope={3}", _clientConfiguration.ClientId, _clientConfiguration.RedirectUri, oAuthFlow, scopes)
            );
            Console.WriteLine("\nYou're prompted to enter your credentials and consent the use of the scopes.");

            Console.WriteLine("Enter username: ");
            string username = Console.ReadLine() ?? throw new InvalidOperationException("Username can't be null");
            Console.WriteLine("Enter password: ");
            string password = Console.ReadLine() ?? throw new InvalidOperationException("Password can't be null");
            Console.WriteLine($"Are you sure you want to provide ${_clientConfiguration.ClientId} full access to your account?");
            string consent = Console.ReadLine() ?? string.Empty;

            if (!consent.StartsWith("y", StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }

            var authorizationCode = _oAuthService.ValidateCredentialsAndGetAuthCode(username, password, _clientConfiguration.ClientId);

            Console.WriteLine($"You're being redirected to: {_clientConfiguration.RedirectUri}?authorization_code={authorizationCode}");
            Console.WriteLine("Client (frontend) sends token to API, and API receives it.");
            Console.WriteLine("Api Contacts the IDP server and ask to exchange the authorization code for an access token, using its OAuth Credentials.");

            var accesToken = _oAuthService.GetAsymmetricAuthToken(authorizationCode, _clientConfiguration);
            Console.WriteLine("JWT Token: " + accesToken);

            Console.WriteLine("The API now needs to validate that the token really comes from the IDP (in this case the one who grants and bear the token are different servers).");
            
            if (ValidateToken(accesToken))
            {
                _encodedJWTToken = accesToken;
                Console.WriteLine("Confirmed JWT token is valid.");
            } else
            {
                Console.WriteLine("The JWT has been tampered with.");
                return;
            }
        }

        private bool ValidateToken(string encodedToken)
        {
            var tokenParts = encodedToken.Split('.').Take(2).Select(x => Base64UrlEncoder.Decode(x)).ToArray();
            var signature = Base64UrlEncoder.DecodeBytes(encodedToken.Split('.').Last());

            var token = new Token.JWTToken
            {
                Header = JsonSerializer.Deserialize<Token.TokenHeader>(tokenParts[0]) ?? throw new InvalidOperationException("Could not parse the JWT header."),
                Payload = JsonSerializer.Deserialize<Token.Payload>(tokenParts[1]) ?? throw new InvalidOperationException("Could not parse the JWT payload."),
                Signature = signature
            };

            var jwks = _oAuthService.GetJWKS();
            var publicKey = jwks.FirstOrDefault(x => x.kid == token.Header.Kid).key ?? throw new Exception("Could not retrieve public key.");

            // When we encrypt this payload and header, the signing key should match the original one.
            using RSA rsa = RSA.Create();
            rsa.ImportRSAPublicKey(publicKey, out _);

            var encodedHeader = Base64UrlEncoder.Encode(JsonSerializer.Serialize(token.Header));
            var encodedPayload = Base64UrlEncoder.Encode(JsonSerializer.Serialize(token.Payload));

            bool isValid = rsa.VerifyData(
                Encoding.UTF8.GetBytes($"{encodedHeader}.{encodedPayload}"),
                token.Signature,
                HashAlgorithmName.SHA256, // This is found in the token header in Alg
                RSASignaturePadding.Pkcs1
            );

            return isValid;
        }
    }
}
