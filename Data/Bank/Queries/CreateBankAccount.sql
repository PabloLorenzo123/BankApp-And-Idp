BEGIN TRANSACTION;
	INSERT INTO "accounts" ("user_id") VALUES (@UserId);

	-- Let's give the new account a bonus of 10,000 dollars.
	INSERT INTO "transactions" ("amount", "type", "date", "account_id")
	VALUES (10000, 'DEPOSIT', current_timestamp, (SELECT "account_id" FROM "accounts" WHERE "user_id" = @UserId));
COMMIT;