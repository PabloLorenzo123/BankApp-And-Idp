SELECT
"account_id" AS AccountId,
"user_id" AS UserId
FROM "accounts"
WHERE account_id = @AccountId;