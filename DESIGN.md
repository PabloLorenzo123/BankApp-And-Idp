# Design Document

By Pablo Lorenzo

Video overview: <URL HERE>

## Scope

This system uses two databases, this is a bank application. Most bank applications have and identity and resource server, on this project idp.db is the database
for the identity server and bank.db is the resource server, the purpose of this system is for users to log in to their bank accounts and perform transactions.
This functionality is achieved by using code that execute raw queries saved in `Data/Bank/Queries` and `Data/Idp/Queries`.

## Functional Requirements

In this section you should answer the following questions:

* Users should be able to sign up into the Banks identity provider.
* The Bank Application should be able to create an OAuth2.0 client in the identity provider database in order to get identity tokens from users.
* Users should be able to login to their bank account using their idp account using OPEN ID CONNECT.
* Users should be able to perform money transactions.
* Users should be able to see their balance.
* Users should be able to see the application logs.

## Representation

### Entities

In this section you should answer the following questions:

* Which entities will you choose to represent in your database?
* What attributes will those entities have?
* Why did you choose the types you did?
* Why did you choose the constraints you did?

### Relationships

In this section you should include your entity relationship diagram and describe the relationships between the entities in your database.

## Optimizations

In this section you should answer the following questions:

* Which optimizations (e.g., indexes, views) did you create? Why?

## Limitations

In this section you should answer the following questions:

* What are the limitations of your design?
* What might your database not be able to represent very well?
