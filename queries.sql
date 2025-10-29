-- Bank
-- Create Bank Account
BEGIN TRANSACTION;
	INSERT INTO "accounts" ("user_id") VALUES (@UserId);

	-- Let's give the new account a bonus of 10,000 dollars.
	INSERT INTO "transactions" ("amount", "type", "date", "account_id")
	VALUES (10000, 'DEPOSIT', current_timestamp, (SELECT "account_id" FROM "accounts" WHERE "user_id" = @UserId));
COMMIT;

-- Get bank account's balance.
WITH
debit AS (
	SELECT
	COALESCE(SUM("transactions"."amount"), 0) + COALESCE(SUM("transfers"."amount"), 0) AS 'amount',
	"accounts"."account_id" AS 'AccountId'
	FROM "accounts"
	LEFT JOIN "transactions" ON "transactions"."account_id" = "accounts"."account_id" AND type = 'DEPOSIT'
	LEFT JOIN "transfers" 	 ON "transfers"."receiver_id" = "accounts"."account_id"
	WHERE "accounts"."account_id" = @AccountId
	GROUP BY "accounts"."account_id"
),
credit AS (
	SELECT
	COALESCE(SUM("transactions"."amount"), 0) + COALESCE(SUM("transfers"."amount"), 0) AS 'amount',
	"accounts"."account_id" AS 'AccountId'
 	FROM "accounts"
	LEFT JOIN "transactions" ON "transactions"."account_id" = "accounts"."account_id"AND type = 'WITHDRAWAL'
	LEFT JOIN "transfers" 	 ON "transfers"."sender_id" = "accounts"."account_id"
	WHERE "accounts"."account_id" = @AccountId
	GROUP BY "accounts"."account_id"
)

SELECT
("debit"."amount" - "credit"."amount") AS 'Balance'
FROM "debit"
JOIN "credit" ON "debit"."AccountId" = "credit"."AccountId";

-- Transfer money to another account.
INSERT INTO "transfers" ("amount", "sender_id", "receiver_id", "date")
VALUES (@Amount, @SenderId, @ReceiverId, @Date);

-- IDP
-- Create user and assing role
BEGIN TRANSACTION;
	-- Create user
	INSERT INTO "USERS" ("username", "password_hash", "password_salt") VALUES (LOWER(@Username), @PasswordHash, @PasswordSalt);
	-- Assign Role
	INSERT INTO"USERS_ROLES" VALUES (
		(SELECT "user_id" FROM "USERS" WHERE "username" = @Username),
		(SELECT "role_id" FROM "ROLES" WHERE "name" LIKE 'customer')
	);
COMMIT;
