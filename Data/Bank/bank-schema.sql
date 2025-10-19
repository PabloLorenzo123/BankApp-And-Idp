-- Online Banking System --
CREATE TABLE "accounts" (
	"account_id" INTEGER,
	"user_id" INTEGER NOT NULL,
	PRIMARY KEY("account_id")
);

CREATE TABLE "transactions" (
	"transaction_id" INTEGER,
	"amount" INTEGER,
	"account_id" INTEGER,
	"type" VARCHAR(250) CHECK("type" in ("DEPOSIT", "WITHDRAWAL")),
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
	FOREIGN KEY ("receiver_id") REFERENCES "accounts"("accounts_id")
);
