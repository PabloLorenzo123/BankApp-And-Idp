using System.Security.Cryptography;
using System.Text;
using Data.IDP.Entities;
using Data.Extensions;
using Data;
using IDP.DTOs;

namespace IDP.Repositories
{
    public class UsersRepository(ConnectionFactory connectionFactory)
    {   
        public IEnumerable<User> GetAll()
        {
            using var connection = connectionFactory.CreateConnection(Connections.IDP);
            connection.Open();
            return connection.QueryFromFile<User>(Queries.IDP.GetUsers, null);
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

            using var connection = connectionFactory.CreateConnection(Connections.IDP);
            connection.Open();
            connection.ExecuteFromFile(Queries.IDP.RegisterUser, newUser);
            return connection.QuerySingleFromFile<User>(Queries.IDP.QueryUserByUsername, new { createUserDto.Username });
        }

        public User Get(string username)
        {
            using var connection = connectionFactory.CreateConnection(Connections.IDP);
            connection.Open();
            return connection.QuerySingleFromFile<User>(Queries.IDP.QueryUserByUsername, new { Username = username });
        }

        public User Get(int id)
        {
            using var connection = connectionFactory.CreateConnection(Connections.IDP);
            connection.Open();
            return connection.QuerySingleFromFile<User>(Queries.IDP.GetUserById, new { Id = id });
        }

        public IEnumerable<string> GetUserRoles(User user)
        {
            using var connection = connectionFactory.CreateConnection(Connections.IDP);
            connection.Open();
            return connection.QueryFromFile<string>(Queries.IDP.GetUserRoles, new {UserId = user.Id});
        }
    }
}
