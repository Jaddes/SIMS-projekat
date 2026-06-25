using SIMSProject.Models;
using SIMSProject.ServiceInterfaces;
using SIMSProject.Utils;

namespace SIMSProject.Menus;

public class LoginMenu(IAuthService authService)
{
    public User? Login()
    {
        ConsoleHelper.PrintHeader("Prijava na sistem");

        var credentials = new LoginCredentials
        {
            Email = InputValidator.ReadRequiredString("Email: "),
            Password = InputValidator.ReadRequiredString("Lozinka: ")
        };

        return authService.Login(credentials);
    }
}
