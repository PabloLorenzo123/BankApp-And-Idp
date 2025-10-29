using IDP;
using IDP.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Memento
{
    public class Authentication
    {
        public string _encodedJWTToken = string.Empty;
        private readonly JsonSerializerOptions jsonSerializerOptions = new() { WriteIndented = true };
        private readonly OAuthClientConfiguration _clientConfiguration = new()
        {
            ClientId = "bankApp",
            ClientSecret = "secret",
            RedirectUri = "https://bank-app.com/user-signed-in"
        };
        private readonly IDPApi _idpApi;

        public Authentication(IDPApi idpApi)
        {
            _idpApi = idpApi;
            _idpApi.RegisterOAuthClient(_clientConfiguration); // Register OAuthClient.
        }

        /// <summary>
        /// Implements the authorization code flow of OAuth 2.0. in this case openid because it only proves identity.
        /// </summary>
        public string GetIdentityTokenUsingAuthCodeFlow()
        {
            var oAuthFlow = "authorization_code";
            var scopes = string.Join(";", ["openid"]);

            Console.WriteLine("Starting Authorization Code Flow...");
            Console.WriteLine("The client is being redirected to: ");
            Console.WriteLine(
                string.Format("\nidentity-provider.com?client_id={0}&redirect_uri={1}&response_type={2}&scope={3}", _clientConfiguration.ClientId, _clientConfiguration.RedirectUri, oAuthFlow, scopes)
            );
            Console.WriteLine("\nYou're prompted to enter your credentials and consent the use of the scopes.");

            string username = Utils.PromptText("Enter username: ");
            string password = Utils.PromptText("Enter password: ");

            Console.WriteLine($"Are you sure you want to provide {_clientConfiguration.ClientId} access to your account's identity?");
            string consent = Console.ReadLine() ?? string.Empty;

            if (!consent.StartsWith("y", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("User needs to give its consent");
            }

            var authorizationCode = _idpApi.ValidateCredentialsAndGetAuthCode(username, password, _clientConfiguration.ClientId, scopes);

            Console.WriteLine($"You're being redirected to: {_clientConfiguration.RedirectUri}?authorization_code={authorizationCode}");
            Console.WriteLine("Client (frontend) sends token to API, and API receives it.");
            Console.WriteLine("Api Contacts the IDP server and ask to exchange the authorization code for an identity token, using its OAuth Credentials.");

            var idToken = _idpApi.GetAsymmetricAuthToken(authorizationCode, _clientConfiguration);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nJWT Token Base4url: " + idToken);
            Console.WriteLine("\nDeserealized token:");
            Console.WriteLine(JsonSerializer.Serialize(DeseralizeToken(idToken), jsonSerializerOptions));
            Console.ResetColor();

            Console.WriteLine("The API now needs to validate that the token really comes from the IDP (in this case the one who grants and bear the token are different servers).");
            
            if (ValidateToken(idToken))
            {
                _encodedJWTToken = idToken;
                return idToken;
            } else
            {
                throw new Exception("The JWT has been tampered with.");
            }
        }

        public Token.JWTToken DeseralizeToken(string encodedToken)
        {
            var tokenParts = encodedToken.Split('.').Take(2).Select(x => Base64UrlEncoder.Decode(x)).ToArray();
            var signature = Base64UrlEncoder.DecodeBytes(encodedToken.Split('.').Last());

            return new Token.JWTToken
            {
                Header = JsonSerializer.Deserialize<Token.TokenHeader>(tokenParts[0]) ?? throw new InvalidOperationException("Could not parse the JWT header."),
                Payload = JsonSerializer.Deserialize<Token.Payload>(tokenParts[1]) ?? throw new InvalidOperationException("Could not parse the JWT payload."),
                Signature = signature
            };
        }
        private bool ValidateToken(string encodedToken)
        {
            var token = DeseralizeToken(encodedToken);

            var jwks = _idpApi.GetJWKS();
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
