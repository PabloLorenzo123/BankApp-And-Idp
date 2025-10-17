CREATE TABLE "USER" (
	"user_id" INTEGER,
	"username" VARCHAR(255) NOT NULL,
	"password_hash" BLOB NOT NULL,
	"password_salt" BLOB NOT NULL,
	PRIMARY KEY ("user_id")
);

CREATE TABLE OAUTH_CLIENT (
	client_id VARCHAR(255),
	client_secret VARCHAR(255) NOT NULL,
	PRIMARY KEY(client_id)
);

CREATE TABLE AUTHORIZATION_CODE (
	"authorization_code" varchar(255),
	"oauth_client_id" VARCHAR(255),
	"user_id" INT,
	PRIMARY KEY ("authorization_code"),
	FOREIGN KEY ("oauth_client_id") REFERENCES "OAUTH_CLIENT"("client_id"),
	FOREIGN KEY ("user_id") REFERENCES "USER"("user_id")
);

