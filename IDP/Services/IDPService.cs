using IDP.Entities;
using IDP.Entities.DTOs;
using IDP.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IDP.Services
{
    public class IDPService(UsersRepository usersRepository)
    {
        private readonly UsersRepository _usersRepository = usersRepository;

        public User SignUp()
        {
            Console.WriteLine("Sign Up");
            // Prompting
            Console.WriteLine("Enter username: ");
            var username = Console.ReadLine() ?? throw new InvalidOperationException("Username can't be null");
            Console.WriteLine("Enter password: ");
            var password = Console.ReadLine() ?? throw new InvalidOperationException("Password can't be null");

            // Using repository
            var user = _usersRepository.Create(new CreateUserDto { Username = username, Password = password });
            Console.WriteLine($"User {user.Username} created with ID {user.Id}");
            return user;
        }
    }
}
