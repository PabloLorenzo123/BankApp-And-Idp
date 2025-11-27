-- Authorization and Identity --
CREATE TABLE IF NOT EXISTS "users" (
	"user_id" INTEGER,
	"username" VARCHAR(255) NOT NULL,
	"password_hash" BLOB NOT NULL,
	"password_salt" BLOB NOT NULL,
	PRIMARY KEY ("user_id")
);

CREATE TABLE IF NOT EXISTS "users_roles" (
	"user_id" INTEGER,
	"role_id" INTEGER,
	PRIMARY KEY ("user_id", "role_id"),
	CONSTRAINT "ROLE_ASSOCIATED_TO_USER"
		FOREIGN KEY ("user_id") REFERENCES "users"("user_id"),
	CONSTRAINT "FK_ROLE"
		FOREIGN KEY ("role_id") REFERENCES "roles"("role_id")
);

CREATE TABLE IF NOT EXISTS "roles" (
	"role_id" INTEGER,
	"name" VARCHAR(255) NOT NULL,
	PRIMARY KEY ("role_id")
);

CREATE TABLE IF NOT EXISTS "roles_claims" (
	"role_id" INTEGER,
	"claim_id" INTEGER,
	PRIMARY KEY ("role_id", "claim_id"),
	CONSTRAINT "FK_ROLE"
		FOREIGN KEY ("role_id") REFERENCES "roles"("role_id"),
	CONSTRAINT "FK_CLAIM"
		FOREIGN KEY ("claim_id") REFERENCES "claims"("claim_id")
);

CREATE TABLE IF NOT EXISTS "claims" (
	"claim_id" INTEGER,
	"name" VARCHAR(255) NOT NULL UNIQUE,
	PRIMARY KEY ("claim_id")
);

CREATE TABLE IF NOT EXISTS "oauth_clients" (
	client_id VARCHAR(255),
	client_secret VARCHAR(255) NOT NULL,
	PRIMARY KEY(client_id)
);

CREATE TABLE IF NOT EXISTS "authorization_codes" (
	"authorization_code" varchar(255),
	"oauth_client_id" VARCHAR(255),
	"user_id" INTEGER,
	"scopes" VARCHAR(255),
	PRIMARY KEY ("authorization_code"),
	CONSTRAINT "ASSOCIATED_CLIENT"
		FOREIGN KEY ("oauth_client_id") REFERENCES "oauth_clients"("client_id"),
	CONSTRAINT "USER_TO_GET_TOKEN_FROM"
		FOREIGN KEY ("user_id") REFERENCES "users"("user_id")
);

-- Seed
INSERT INTO "roles" ("name") VALUES ('customer');
INSERT INTO "claims" ("name") VALUES ('can_use_the_bank_app');

INSERT INTO "roles_claims" ("role_id", "claim_id")
VALUES (
	(SELECT "role_id" FROM "roles" WHERE "name" = 'customer'),
	(SELECT "claim_id" FROM "claims" WHERE "name" = 'can_use_the_bank_app')
);
