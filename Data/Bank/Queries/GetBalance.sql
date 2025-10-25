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