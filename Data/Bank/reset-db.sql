DROP TABLE IF EXISTS "transactions";
DROP TABLE IF EXISTS "transfers";
DROP TABLE IF EXISTS"logs";
DROP TABLE IF EXISTS "accounts";

DROP VIEW IF EXISTS "Balance";
DROP VIEW IF EXISTS "bank_accounts";

DROP TRIGGER IF EXISTS "account_soft_delete";
DROP TRIGGER IF EXISTS "account_egress_transaction";
DROP TRIGGER IF EXISTS "account_ingress_transaction";
DROP TRIGGER IF EXISTS "account_transfers";