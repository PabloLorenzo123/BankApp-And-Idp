-- Online Banking System --
CREATE TABLE "accounts" (
	"account_id" INTEGER,
	"user_id" INTEGER NOT NULL,
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
	WHERE "accounts"."account_id" = 1
	GROUP BY "accounts"."account_id"
),
credit AS (
	SELECT
	COALESCE(SUM("transactions"."amount"), 0) + COALESCE(SUM("transfers"."amount"), 0) AS 'amount',
	"accounts"."account_id" AS 'AccountId'
 	FROM "accounts"
	LEFT JOIN "transactions" ON "transactions"."account_id" = "accounts"."account_id"AND type = 'WITHDRAWAL'
	LEFT JOIN "transfers" 	 ON "transfers"."sender_id" = "accounts"."account_id"
	WHERE "accounts"."account_id" = 1
	GROUP BY "accounts"."account_id"
)
SELECT
("debit"."amount" - "credit"."amount") AS 'Balance'
FROM "debit"
JOIN "credit" ON "debit"."AccountId" = "credit"."AccountId";


-- Data Seed

INSERT INTO accounts (account_id, user_id) VALUES
(1, 101), (2, 102), (3, 103), (4, 104), (5, 105),
(6, 106), (7, 107), (8, 108), (9, 109), (10, 110),
(11, 111), (12, 112), (13, 113), (14, 114), (15, 115),
(16, 116), (17, 117), (18, 118), (19, 119), (20, 120);


INSERT INTO transactions (transaction_id, amount, account_id, type) VALUES
(1, 500, 1, 'DEPOSIT'),
(2, 200, 1, 'WITHDRAWAL'),
(3, 1000, 2, 'DEPOSIT'),
(4, 300, 2, 'WITHDRAWAL'),
(5, 1500, 3, 'DEPOSIT'),
(6, 400, 3, 'WITHDRAWAL'),
(7, 700, 4, 'DEPOSIT'),
(8, 200, 5, 'DEPOSIT'),
(9, 100, 5, 'WITHDRAWAL'),
(10, 250, 6, 'DEPOSIT'),
(11, 300, 7, 'DEPOSIT'),
(12, 150, 8, 'WITHDRAWAL'),
(13, 800, 9, 'DEPOSIT'),
(14, 500, 10, 'DEPOSIT'),
(15, 100, 11, 'WITHDRAWAL'),
(16, 400, 12, 'DEPOSIT'),
(17, 700, 13, 'DEPOSIT'),
(18, 200, 14, 'WITHDRAWAL'),
(19, 900, 15, 'DEPOSIT'),
(20, 350, 16, 'DEPOSIT');

INSERT INTO transfers (transfer_id, amount, sender_id, receiver_id) VALUES
(1, 100, 1, 2),
(2, 200, 2, 3),
(3, 150, 3, 4),
(4, 50, 4, 5),
(5, 300, 5, 6),
(6, 100, 6, 7),
(7, 400, 7, 8),
(8, 250, 8, 9),
(9, 200, 9, 10),
(10, 500, 10, 11),
(11, 100, 11, 12),
(12, 350, 12, 13),
(13, 150, 13, 14),
(14, 200, 14, 15),
(15, 400, 15, 16),
(16, 250, 16, 17),
(17, 500, 17, 18),
(18, 100, 18, 19),
(19, 300, 19, 20),
(20, 200, 20, 1);
