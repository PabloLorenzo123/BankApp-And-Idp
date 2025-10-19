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
