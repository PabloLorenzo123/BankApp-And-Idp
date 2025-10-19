SELECT authorization_code as Code,
oauth_client_id as OAuthClientId,
"user_id" as UserId,
scopes as Scopes
FROM AUTHORIZATION_CODES
WHERE code = @Code;