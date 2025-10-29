BEGIN TRANSACTION;
	-- Create user
	INSERT INTO "USERS" ("username", "password_hash", "password_salt") VALUES (LOWER(@Username), @PasswordHash, @PasswordSalt);
	-- Assign Role
	INSERT INTO"USERS_ROLES" VALUES (
		(SELECT "user_id" FROM "USERS" WHERE "username" = @Username),
		(SELECT "role_id" FROM "ROLES" WHERE "name" LIKE 'customer')
	);
COMMIT;