using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDP.Entities.DTOs
{
    /// <summary>
    /// Token header (meta data about the token).
    /// </summary>
    public class TokenHeader
    {
        /// <summary>
        /// What algorithm was used to sign the token. This is useful in cases where the client needs to validate
        /// The token's integrity by using a public key.
        /// </summary>
        public string Alg { get; set; } = string.Empty;

        /// <summary>
        /// Type
        /// </summary>
        public string Typ { get; set; } = "JWT";

        /// <summary>
        /// Key Id, used for apis to retrieve JWKS from the IDP and then using the matching public key to verify the signature/
        /// </summary>
        public string Kid { get; set; } = string.Empty;
    }

    /// <summary>
    /// Token Payload (claims).
    /// </summary>
    public class Payload
    {
        // De Facto Fields.

        /// <summary>
        /// Unique identifier for the user.
        /// </summary>
        public string Sub { get; set; } = string.Empty;

        /// <summary>
        /// Who emitted and signed the token.
        /// </summary>
        public string Iss { get; set; } = string.Empty;

        /// <summary>
        /// Who the token is intended for.
        /// </summary>
        public string Aud { get; set; } = string.Empty;

        /// <summary>
        /// Expiration time (unix timestamp)
        /// </summary>
        public double Exp { get; set; }

        /// <summary>
        /// Don't use token before this unix time.
        /// </summary>
        public double Nbf { get; set; }

        /// <summary>
        /// Issued at time.
        /// </summary>
        public double Iat { get; set; }

        /// <summary>
        /// Jwt unique identifier.
        /// </summary>
        public Guid Jti { get; set; }

        // Additional fields.
        
        /// <summary>
        /// Roles
        /// </summary>
        public string[] Roles { get; set; } = [];

        /// <summary>
        /// Scopes.
        /// </summary>
        public string[] Scopes { get; set; } = [];
    }

    public class JWTToken
    {
        public required TokenHeader Header { get; set; }
        public required Payload Payload { get; set; }
        public byte[] Signature { get; set; } = [];
    }

}
