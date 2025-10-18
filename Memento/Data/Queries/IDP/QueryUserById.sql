SELECT
"user_id" AS "Id",
"username" AS "Username",
"password_hash" AS "PasswordHash",
"password_salt" AS "PasswordSalt"
FROM "USERS"
WHERE "user_id" = @Id;