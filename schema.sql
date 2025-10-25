-- Online Banking System --
CREATE TABLE "accounts" (
	"account_id" INTEGER,
	"user_id" INTEGER NOT NULL,
	"deleted" INTEGER DEFAULT 0,
	PRIMARY KEY("account_id")
);

CREATE TABLE "transactions" (
	"transaction_id" INTEGER,
	"account_id" INTEGER,
	"amount" INTEGER,
	"type" VARCHAR(250) CHECK("type" in ('DEPOSIT', 'WITHDRAWAL')),
	"date" DATETIME DEFAULT CURRENT_TIMESTAMP,
	PRIMARY KEY ("transaction_id"),
	FOREIGN KEY ("account_id") REFERENCES "accounts"("account_id")
);

CREATE TABLE "transfers" (
	"transfer_id" INTEGER,
	"amount" INTEGER NOT NULL,
	"sender_id" INTEGER CHECK("sender_id" != "receiver_id") NOT NULL,
	"receiver_id" INTEGER NOT NULL,
	"date" DATETIME DEFAULT CURRENT_TIMESTAMP,
	PRIMARY KEY("transfer_id"),
	FOREIGN KEY ("sender_id") REFERENCES "accounts"("account_id"),
	FOREIGN KEY ("receiver_id") REFERENCES "accounts"("account_id")
);

CREATE TABLE "logs" (
	"log_id" INTEGER,
	"account_id" INTEGER NOT NULL DEFAULT 0,
	"information" VARCHAR(250) NOT NULL,
	"date" DATETIME DEFAULT CURRENT_TIMESTAMP,
	PRIMARY KEY ("log_id"),
	FOREIGN KEY ("account_id") REFERENCES "accounts"("account_id") ON DELETE SET DEFAULT
);

-- VIEWS
CREATE VIEW "Balance" AS
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

CREATE VIEW "bank_accounts" AS SELECT * FROM "accounts";

-- Triggers
CREATE TRIGGER "account_soft_delete"
INSTEAD OF DELETE ON "bank_accounts"
FOR EACH ROW
BEGIN
	UPDATE "accounts"
	SET "deleted" = 1
	WHERE "accounts"."account_id" = OLD."account_id";
END;

CREATE TRIGGER "account_egress_transaction"
AFTER INSERT ON "transactions"
FOR EACH ROW WHEN NEW."type" = 'WITHDRAWAL'
BEGIN
	INSERT INTO "logs" ("account_id", "information")
	VALUES (
		NEW."account_id", 
		"Account ID: " || NEW."account_id" || " withdraw $" || NEW."amount" || " from its account new balance: " || (SELECT "Balance" FROM "Balance" WHERE "account_id" = NEW."account_id")
	);
END;

CREATE TRIGGER "account_ingress_transaction"
AFTER INSERT ON "transactions"
FOR EACH ROW WHEN NEW."type" = 'DEPOSIT'
BEGIN
	INSERT INTO "logs" ("account_id", "information")
	VALUES (
		NEW."account_id", 
		"Account ID: " || NEW."account_id" || " deposited $" || NEW."amount" || " from its account new balance: " || (SELECT "Balance" FROM "Balance" WHERE "account_id" = NEW."account_id")
	);
END;

CREATE TRIGGER "account_transfers"
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
CREATE INDEX "transaction_by_user"
ON "transactions"("account_id");

CREATE INDEX "logs_date"
ON "logs"("date");

-- IDP
-- Authorization and Identity --
CREATE TABLE "users" (
	"user_id" INTEGER,
	"username" VARCHAR(255) NOT NULL,
	"password_hash" BLOB NOT NULL,
	"password_salt" BLOB NOT NULL,
	PRIMARY KEY ("user_id")
);

CREATE TABLE "users_roles" (
	"user_id" INTEGER,
	"role_id" INTEGER,
	PRIMARY KEY ("user_id", "role_id"),
	FOREIGN KEY ("user_id") REFERENCES "users"("user_id"),
	FOREIGN KEY ("role_id") REFERENCES "roles"("role_id")
);

CREATE TABLE "roles" (
	"role_id" INTEGER,
	"name" VARCHAR(255) NOT NULL,
	PRIMARY KEY ("role_id")
);

CREATE TABLE "roles_claims" (
	"role_id" INTEGER,
	"claim_id" INTEGER,
	PRIMARY KEY ("role_id", "claim_id"),
	FOREIGN KEY ("role_id") REFERENCES "roles"("role_id"),
	FOREIGN KEY ("claim_id") REFERENCES "claims"("claim_id")
);

CREATE TABLE "claims" (
	"claim_id" INTEGER,
	"name" VARCHAR(255) NOT NULL UNIQUE,
	PRIMARY KEY ("claim_id")
);

CREATE TABLE "oauth_clients" (
	client_id VARCHAR(255),
	client_secret VARCHAR(255) NOT NULL,
	PRIMARY KEY(client_id)
);

CREATE TABLE "authorization_codes" (
	"authorization_code" varchar(255),
	"oauth_client_id" VARCHAR(255),
	"user_id" INTEGER,
	"scopes" VARCHAR(255),
	PRIMARY KEY ("authorization_code"),
	FOREIGN KEY ("oauth_client_id") REFERENCES "oauth_clients"("client_id"),
	FOREIGN KEY ("user_id") REFERENCES "users"("user_id")
);
