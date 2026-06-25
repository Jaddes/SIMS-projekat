using SIMSProject.Models;
using SIMSProject.ServiceInterfaces;
using SIMSProject.Utils;

namespace SIMSProject.Menus;

public class MainMenu(
    LoginMenu loginMenu,
    ITenantService tenantService,
    TenantMenu tenantMenu,
    ManagerMenu managerMenu,
    AdminMenu adminMenu)
{
    public void Show()
    {
        var running = true;

        while (running)
        {
            ConsoleHelper.PrintHeader("SIMS Projekat");
            Console.WriteLine("1. Prijava");
            Console.WriteLine("2. Registracija stanara");
            Console.WriteLine("0. Izlaz");
            Console.WriteLine();
            Console.WriteLine("Seed administrator:");
            Console.WriteLine("Email: admin@sims.local");
            Console.WriteLine("Lozinka: Admin123!");
            Console.WriteLine();

            switch (InputValidator.ReadMenuChoice("Izbor: ", 0, 1, 2))
            {
                case 1:
                    HandleLogin();
                    break;
                case 2:
                    RegisterTenant();
                    break;
                case 0:
                    running = false;
                    break;
            }
        }
    }

    private void HandleLogin()
    {
        var user = loginMenu.Login();
        if (user is null)
        {
            ConsoleHelper.PrintError("Pogresan email ili lozinka.");
            ConsoleHelper.Pause();
            return;
        }

        switch (user)
        {
            case Administrator administrator:
                adminMenu.Show(administrator);
                break;
            case BuildingManager manager:
                managerMenu.Show(manager);
                break;
            case Tenant tenant:
                tenantMenu.Show(tenant);
                break;
            default:
                ConsoleHelper.PrintError("Nepoznat tip korisnika.");
                ConsoleHelper.Pause();
                break;
        }
    }

    private void RegisterTenant()
    {
        ConsoleHelper.PrintHeader("Registracija stanara");
        var tenant = new Tenant
        {
            Jmbg = InputValidator.ReadRequiredString("JMBG: "),
            Email = InputValidator.ReadRequiredString("Email: "),
            Password = InputValidator.ReadRequiredString("Lozinka: "),
            FirstName = InputValidator.ReadRequiredString("Ime: "),
            LastName = InputValidator.ReadRequiredString("Prezime: "),
            MobilePhone = InputValidator.ReadRequiredString("Mobilni telefon: ")
        };

        try
        {
            tenantService.RegisterTenant(tenant);
            ConsoleHelper.PrintSuccess("Stanar je uspesno registrovan.");
        }
        catch (Exception exception)
        {
            ConsoleHelper.PrintError(exception.Message);
        }

        ConsoleHelper.Pause();
    }
}
