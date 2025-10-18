using System.Security.Cryptography;
using System.Text;
using Dapper;
using IDP.Entities;
using IDP.Entities.DTOs;
using IDP.Extensions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Memento.Data;

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
            var query = "SELECT user_id, username FROM USER";

            var command = connection.Query<User>(query);

            return command;
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
            try
            {
                connection.Open();
                var query = connection.Execute(
                    @"INSERT INTO USER (username, password_hash, password_salt)
                        VALUES (@Username, @PasswordHash, @PasswordSalt);
                    ", newUser);
                return newUser;
            }
            catch (Exception ex)
            {
                throw new Exception("There was an error trying to register the user " + ex.Message.ToString());
            }
        }

        public User Get(string username)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.QuerySingleFromFile<User>(Queries.IDPQueries.QueryUserByUsername, new { Username = username });
            if (command == null)
            {
                throw new InvalidOperationException("User not found.");
            }
            return command;
        }

        public User Get(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var query = "SELECT user_id as Id, username as Username, password_hash as PasswordHash, password_salt as PasswordSalt FROM USER WHERE user_id = @Id";
            var command = connection.QuerySingleOrDefault<User>(query, new { Id = id });
            return command ?? throw new InvalidOperationException("User not found.");
        }
    }
}
