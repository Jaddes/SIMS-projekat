using SIMSProject.Models;
using SIMSProject.RepositoryInterfaces;
using SIMSProject.ServiceInterfaces;

namespace SIMSProject.Services;

public class AuthService(IUserRepository userRepository) : IAuthService
{
    public User? Login(LoginCredentials credentials)
    {
        return userRepository
            .GetAll()
            .FirstOrDefault(user =>
                string.Equals(user.Email, credentials.Email, StringComparison.OrdinalIgnoreCase) &&
                user.Password == credentials.Password);
    }
}
