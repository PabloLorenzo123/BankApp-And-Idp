CREATE TABLE USERS (
	id INT,
	username VARCHAR(255) NOT NULL,
	password_hash BLOB NOT NULL,
	password_salt BLOB NOT NULL,
	PRIMARY KEY (ID)
);