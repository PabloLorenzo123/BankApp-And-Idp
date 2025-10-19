using System.Security.Cryptography;
using System.Text;
using Data.IDP.Entities;
using Data.Extensions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Data;
using IDP.DTOs;

namespace IDP.Repositories
{
    public class UsersRepository(IConfiguration configuration)
    {
        private readonly string _connectionString = configuration.GetConnectionString("IDP")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        
        public IEnumerable<User> GetAll()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection.QueryFromFile<User>(Queries.IDPQueries.GetUsers, null);
        }

        public User Create(CreateUserDto createUserDto)
        {
            var hasher = new HMACSHA512(); // Mixes message authentication and SHA512 crytography hashing function.
            var newUser = new User
            {
                Username = createUserDto.Username,
                PasswordHash = hasher.ComputeHash(Encoding.UTF8.GetBytes(createUserDto.Password)),
                PasswordSalt = hasher.Key
            };

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            connection.ExecuteFromFile(Queries.IDPQueries.RegisterUser, newUser);
            return connection.QuerySingleFromFile<User>(Queries.IDPQueries.QueryUserByUsername, new { createUserDto.Username });
        }

        public User Get(string username)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection.QuerySingleFromFile<User>(Queries.IDPQueries.QueryUserByUsername, new { Username = username });
        }

        public User Get(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection.QuerySingleFromFile<User>(Queries.IDPQueries.GetUserById, new { Id = id });
        }
    }
}
