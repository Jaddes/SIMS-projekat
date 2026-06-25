using SIMSProject.Models;
using SIMSProject.ServiceInterfaces;
using SIMSProject.Utils;

namespace SIMSProject.Menus;

public class AdminMenu(
    SharedMenu sharedMenu,
    IAdminService adminService)
{
    public void Show(Administrator administrator)
    {
        var running = true;

        while (running)
        {
            ConsoleHelper.PrintHeader($"Administrator meni - {administrator.FirstName} {administrator.LastName}");
            Console.WriteLine("1. Prikaz odobrenih zgrada");
            Console.WriteLine("2. Pretraga zgrada");
            Console.WriteLine("3. Registracija upravnika");
            Console.WriteLine("4. Unos zgrade");
            Console.WriteLine("0. Odjava");

            switch (InputValidator.ReadMenuChoice("Izbor: ", 0, 1, 2, 3, 4))
            {
                case 1:
                    sharedMenu.ShowApprovedBuildings();
                    break;
                case 2:
                    sharedMenu.SearchBuildings();
                    break;
                case 3:
                    RegisterManager();
                    break;
                case 4:
                    CreateBuilding();
                    break;
                case 0:
                    running = false;
                    break;
            }
        }
    }

    private void RegisterManager()
    {
        ConsoleHelper.PrintHeader("Registracija upravnika");
        var manager = new BuildingManager
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
            adminService.RegisterManager(manager);
            ConsoleHelper.PrintSuccess("Upravnik je uspesno registrovan.");
        }
        catch (Exception exception)
        {
            ConsoleHelper.PrintError(exception.Message);
        }

        ConsoleHelper.Pause();
    }

    private void CreateBuilding()
    {
        ConsoleHelper.PrintHeader("Unos zgrade");
        var building = new Building
        {
            Code = InputValidator.ReadRequiredString("Sifra zgrade: "),
            Address = new Address
            {
                Street = InputValidator.ReadRequiredString("Ulica: "),
                Number = InputValidator.ReadRequiredString("Broj: ")
            },
            Neighborhood = InputValidator.ReadRequiredString("Naselje: "),
            Location = new Location
            {
                City = InputValidator.ReadRequiredString("Grad: "),
                Country = InputValidator.ReadRequiredString("Drzava: ")
            },
            FloorCount = InputValidator.ReadInt("Broj spratova: ", 0),
            ManagerJmbg = InputValidator.ReadRequiredString("JMBG upravnika: ")
        };

        try
        {
            adminService.CreateBuilding(building);
            ConsoleHelper.PrintSuccess("Zgrada je uneta i ceka odobrenje upravnika.");
        }
        catch (Exception exception)
        {
            ConsoleHelper.PrintError(exception.Message);
        }

        ConsoleHelper.Pause();
    }
}
