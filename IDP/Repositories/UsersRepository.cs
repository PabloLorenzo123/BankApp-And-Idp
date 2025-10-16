using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using IDP.Entities;
using IDP.Entities.DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace IDP.Repositories
{
    public class UsersRepository(IConfiguration configuration)
    {
        private readonly string _connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        
        public IEnumerable<User> GetAll()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var query = "SELECT Id, Username, PasswordHash, PasswordKey FROM Users";

            var command = connection.Query<User>(query);

            return command;
        }


        public User Create(CreateUserDto createUserDto)
        {
            var hasher = new HMACSHA512(); // Mixes message authentication and SHA512 crytography hashing function.
            var newUser = new User
            {
                Username = createUserDto.Username,
                PasswordHash = SHA256.HashData(Encoding.UTF8.GetBytes(createUserDto.Password)),
                PasswordSalt = hasher.Key
            };

            using var connection = new SqliteConnection(_connectionString);
            try
            {
                connection.Open();
                var query = connection.Query(
                    @"INSERT INTO USERS (username, password_hash, password_salt)
                        VALUES (@Username, @PasswordHash, @PasswordSalt);
                    ", newUser);
                return newUser;
            }
            catch (Exception ex)
            {

                throw new Exception("There was an error trying to register the user " + ex.Message.ToString());
            }
        }
    }
}
