using IDP.Entities;
using IDP.Entities.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IDP.Services
{
    public class TokenGenerator
    {
        private readonly string _connectionString;

        private const string _symmetricSigningKey = "key-used-for-both-signing-and-validating";
        
        public readonly byte[] publicKey;
        private readonly byte[] _privateKey;

        public TokenGenerator(IConfiguration configuration)
        {
            (publicKey, _privateKey) = GenerateKeyPair();
            _connectionString = configuration.GetConnectionString("default") ?? throw new ArgumentNullException(nameof(configuration));
        }

        public (Token.JWTToken token, string encodedToken) CreateAuthTokenUsingSymmetricSigning(User user, AuthorizationCode authCode)
        {
            var hasher = new HMACSHA256(Encoding.UTF8.GetBytes(_symmetricSigningKey)); // Use a Message Encoding Algorithm to avoid tampering, and promote integrity.

            var header = new Token.TokenHeader
            {
                Alg = "HS256", // HMAC with SHA256, HASHED MESSAGE AUTHENTICATION CODE USING SHA-256 -> Use SHA-256 + a shared secret to create a signature.
                Typ = "JWT"
            };
            var encodedHeader = Base64UrlEncoder.Encode(JsonSerializer.Serialize(header)); // Needs to be 64 URL encoded in order to be transmitted through the web.

            var payload = new Token.Payload
            {
                Iss = "Photos App",
                Aud = "Memento",
                Sub = user.Username,
                Iat = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds(),
                Exp = ((DateTimeOffset)DateTime.UtcNow.AddHours(24)).ToUnixTimeSeconds(),
                Jti = Guid.NewGuid(),
                Roles = [],
                Scopes = authCode.Scopes.Split(";")
            };
            var encodedPayload = Base64UrlEncoder.Encode(JsonSerializer.Serialize(payload));


            var signature = hasher.ComputeHash(Encoding.UTF8.GetBytes($"{encodedHeader}.{encodedPayload}"));
            var encodedSignature = Base64UrlEncoder.Encode(signature);
            var token = new Token.JWTToken { Header = header, Payload = payload, Signature = signature };

            var encodedToken = $"{encodedHeader}.{encodedPayload}.{encodedSignature}";

            return (token, encodedToken);
        }

        public (Token.JWTToken token, string encodedToken) CreateAuthTokenUsingAsymmetricSigning(User user, AuthorizationCode authCode)
        {
            using RSA rsaForSigning = RSA.Create();
            rsaForSigning.ImportRSAPrivateKey(_privateKey, out _);

            var header = new Token.TokenHeader
            {
                Alg = "RS256", // RSA + SHA256.
                Kid = "KEY-1",
                Typ = "JWT"
            };

            var encodedHeader = Base64UrlEncoder.Encode(JsonSerializer.Serialize(header));

            var payload = new Token.Payload
            {
                Iss = "Photos App",
                Aud = "Memento",
                Sub = user.Username,
                Iat = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds(),
                Exp = ((DateTimeOffset)DateTime.UtcNow.AddHours(24)).ToUnixTimeSeconds(),
                Jti = Guid.NewGuid(),
                Roles = [],
                Scopes = authCode.Scopes.Split(";")
            };
            var encodedPayload = Base64UrlEncoder.Encode(JsonSerializer.Serialize(payload));

            var signature = rsaForSigning.SignData(
                Encoding.UTF8.GetBytes($"{encodedHeader}.{encodedPayload}"),
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1
            );
            var encodedSignature = Base64UrlEncoder.Encode(signature);

            var token = new Token.JWTToken { Header = header, Payload = payload, Signature = signature };
            var encodedToken = $"{encodedHeader}.{encodedPayload}.{encodedSignature}";

            return (token, encodedToken);
        }

        private static (byte[], byte[]) GenerateKeyPair()
        {
            using RSA rsa = RSA.Create(2048); // 2048 bit key size.
            var publicKey = rsa.ExportRSAPublicKey(); // Used for verifyng signature hasn't been tampered with, i takes the input hasheds it with the key and compare values.
            var privateKey = rsa.ExportRSAPrivateKey(); // Used for creating the signature.
            return (publicKey, privateKey);
        }
    }
}
