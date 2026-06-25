using SIMSProject.Models;

namespace SIMSProject.ServiceInterfaces;

public interface IAuthService
{
    User? Login(LoginCredentials credentials);
}
