DROP TABLE "transactions";
DROP TABLE "transfers";
DROP TABLE "logs";
DROP TABLE "accounts";

DROP VIEW "Balance";
DROP VIEW "bank_accounts";

DROP TRIGGER "account_soft_delete";
DROP TRIGGER "account_egress_transaction";
DROP TRIGGER "account_ingress_transaction";
DROP TRIGGER "account_transfers";