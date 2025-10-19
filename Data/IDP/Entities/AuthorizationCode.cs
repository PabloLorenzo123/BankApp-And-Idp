namespace Data.IDP.Entities
{
    public class AuthorizationCode
    {
        /// <summary>
        /// Identifier, this is the code the client uses to exchange it by an access or an identity token.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// The user who authenticated
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// OAuth Client Id
        /// </summary>
        public string OAuthClientId { get; set; } = string.Empty;

        /// <summary>
        /// Defines the permissions and access levels the bearer is granted.
        /// </summary>
        public string Scopes { get; set; } = string.Empty;
    }
}
