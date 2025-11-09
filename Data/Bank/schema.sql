-- Online Banking System --
CREATE TABLE IF NOT EXISTS "accounts" (
	"account_id" INTEGER,
	"user_id" INTEGER NOT NULL,
	"deleted" INTEGER DEFAULT 0,
	PRIMARY KEY("account_id")
);

CREATE TABLE IF NOT EXISTS "transactions" (
	"transaction_id" INTEGER,
	"account_id" INTEGER,
	"amount" INTEGER,
	"type" VARCHAR(250) CHECK("type" in ('DEPOSIT', 'WITHDRAWAL')),
	"date" DATETIME DEFAULT CURRENT_TIMESTAMP,
	PRIMARY KEY ("transaction_id"),
	FOREIGN KEY ("account_id") REFERENCES "accounts"("account_id")
);

CREATE TABLE IF NOT EXISTS "transfers" (
	"transfer_id" INTEGER,
	"amount" INTEGER NOT NULL,
	"sender_id" INTEGER CHECK("sender_id" != "receiver_id") NOT NULL,
	"receiver_id" INTEGER NOT NULL,
	"date" DATETIME DEFAULT CURRENT_TIMESTAMP,
	PRIMARY KEY("transfer_id"),
	FOREIGN KEY ("sender_id") REFERENCES "accounts"("account_id"),
	FOREIGN KEY ("receiver_id") REFERENCES "accounts"("account_id")
);

CREATE TABLE IF NOT EXISTS "logs" (
	"log_id" INTEGER,
	"account_id" INTEGER NOT NULL DEFAULT 0,
	"information" VARCHAR(250) NOT NULL,
	"date" DATETIME DEFAULT CURRENT_TIMESTAMP,
	PRIMARY KEY ("log_id"),
	FOREIGN KEY ("account_id") REFERENCES "accounts"("account_id") ON DELETE SET DEFAULT
);

-- VIEWS
CREATE VIEW IF NOT EXISTS "Balance" AS
WITH
debit AS (
	SELECT
	COALESCE(SUM("transactions"."amount"), 0) + COALESCE(SUM("transfers"."amount"), 0) AS 'amount',
	"accounts"."account_id" AS 'AccountId'
	FROM "accounts"
	LEFT JOIN "transactions" ON "transactions"."account_id" = "accounts"."account_id"AND type = 'DEPOSIT'
	LEFT JOIN "transfers" 	 ON "transfers"."receiver_id" = "accounts"."account_id"
	GROUP BY "accounts"."account_id"
),
credit AS (
	SELECT
	COALESCE(SUM("transactions"."amount"), 0) + COALESCE(SUM("transfers"."amount"), 0) AS 'amount',
	"accounts"."account_id" AS 'AccountId'
 	FROM "accounts"
	LEFT JOIN "transactions" ON "transactions"."account_id" = "accounts"."account_id"AND type = 'WITHDRAWAL'
	LEFT JOIN "transfers" 	 ON "transfers"."sender_id" = "accounts"."account_id"
	GROUP BY "accounts"."account_id"
)
SELECT
"debit"."AccountId" AS "account_id",
("debit"."amount" - "credit"."amount") AS 'Balance'
FROM "debit"
JOIN "credit" ON "debit"."AccountId" = "credit"."AccountId";

CREATE VIEW IF NOT EXISTS "bank_accounts" AS SELECT * FROM "accounts";

-- Triggers
CREATE TRIGGER IF NOT EXISTS "account_soft_delete"
INSTEAD OF DELETE ON "bank_accounts"
FOR EACH ROW
BEGIN
	UPDATE "accounts"
	SET "deleted" = 1
	WHERE "accounts"."account_id" = OLD."account_id";
END;

CREATE TRIGGER IF NOT EXISTS "account_egress_transaction"
AFTER INSERT ON "transactions"
FOR EACH ROW WHEN NEW."type" = 'WITHDRAWAL'
BEGIN
	INSERT INTO "logs" ("account_id", "information")
	VALUES (
		NEW."account_id", 
		"Account ID: " || NEW."account_id" || " withdraw $" || NEW."amount" || " from its account new balance: " || (SELECT "Balance" FROM "Balance" WHERE "account_id" = NEW."account_id")
	);
END;

CREATE TRIGGER IF NOT EXISTS "account_ingress_transaction"
AFTER INSERT ON "transactions"
FOR EACH ROW WHEN NEW."type" = 'DEPOSIT'
BEGIN
	INSERT INTO "logs" ("account_id", "information")
	VALUES (
		NEW."account_id", 
		"Account ID: " || NEW."account_id" || " deposited $" || NEW."amount" || " from its account new balance: " || (SELECT "Balance" FROM "Balance" WHERE "account_id" = NEW."account_id")
	);
END;

CREATE TRIGGER IF NOT EXISTS "account_transfers"
AFTER INSERT ON "transfers"
FOR EACH ROW
BEGIN
	INSERT INTO "logs" ("account_id", "information")
	VALUES (
		NEW."sender_id", 
		"Account ID: " || NEW."sender_id" || " transfered $" || NEW."amount" || " to: " || "Account ID: " || NEW."receiver_id" || " new balance: " || (SELECT "Balance" FROM "Balance" WHERE "account_id" = NEW."sender_id")
	);

	INSERT INTO "logs" ("account_id", "information")
	VALUES (
		NEW."receiver_id", 
		"Account ID: " || NEW."receiver_id" || " received from a transfer $" || NEW."amount" || " from: " || "Account ID: " || NEW."sender_id" || " new balance: " || (SELECT "Balance" FROM "Balance" WHERE "account_id" = NEW."receiver_id")
	);
END;


-- Indexes.
CREATE INDEX IF NOT EXISTS "transaction_by_user"
ON "transactions"("account_id");

CREATE INDEX IF NOT EXISTS "logs_date"
ON "logs"("date");
