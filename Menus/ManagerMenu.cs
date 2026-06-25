using SIMSProject.Enums;
using SIMSProject.Models;
using SIMSProject.ServiceInterfaces;
using SIMSProject.Utils;

namespace SIMSProject.Menus;

public class ManagerMenu(
    SharedMenu sharedMenu,
    IManagerService managerService)
{
    public void Show(BuildingManager manager)
    {
        var running = true;

        while (running)
        {
            ConsoleHelper.PrintHeader($"Upravnik meni - {manager.FirstName} {manager.LastName}");
            Console.WriteLine("1. Prikaz odobrenih zgrada");
            Console.WriteLine("2. Pretraga zgrada");
            Console.WriteLine("3. Prikaz zahteva");
            Console.WriteLine("4. Obrada zahteva na cekanju");
            Console.WriteLine("5. Prikaz mojih zgrada");
            Console.WriteLine("6. Obrada zgrada na cekanju");
            Console.WriteLine("7. Unos stana");
            Console.WriteLine("0. Odjava");

            switch (InputValidator.ReadMenuChoice("Izbor: ", 0, 1, 2, 3, 4, 5, 6, 7))
            {
                case 1:
                    sharedMenu.ShowApprovedBuildings();
                    break;
                case 2:
                    sharedMenu.SearchBuildings();
                    break;
                case 3:
                    ShowRequests(manager);
                    break;
                case 4:
                    ProcessRequest(manager);
                    break;
                case 5:
                    ShowBuildings(manager);
                    break;
                case 6:
                    ProcessBuilding(manager);
                    break;
                case 7:
                    AddApartment(manager);
                    break;
                case 0:
                    running = false;
                    break;
            }
        }
    }

    private void ShowRequests(BuildingManager manager)
    {
        ConsoleHelper.PrintHeader("Zahtevi upravnika");
        var buildingCode = SelectApprovedBuildingCode(manager);
        if (buildingCode is null)
        {
            return;
        }

        Console.WriteLine("1. Na cekanju");
        Console.WriteLine("2. Potvrdjeni");

        var filter = InputValidator.ReadMenuChoice("Izbor: ", 1, 2) == 1
            ? ManagerRequestFilter.Pending
            : ManagerRequestFilter.Approved;

        var requests = managerService.GetManagerRequests(manager.Jmbg, buildingCode, filter);
        if (requests.Count == 0)
        {
            Console.WriteLine("Nema zahteva za prikaz.");
            ConsoleHelper.Pause();
            return;
        }

        var rows = requests
            .Select(request => new[]
            {
                request.Id,
                request.TenantJmbg,
                request.BuildingCode,
                request.ApartmentNumber.ToString(),
                request.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                request.Status.ToString()
            })
            .ToList();

        TablePrinter.Print(["Id", "TenantJmbg", "Building", "Apartment", "Created", "Status"], rows);
        ConsoleHelper.Pause();
    }

    private void ProcessRequest(BuildingManager manager)
    {
        ConsoleHelper.PrintHeader("Obrada zahteva");
        var buildingCode = SelectApprovedBuildingCode(manager);
        if (buildingCode is null)
        {
            return;
        }

        var requests = managerService.GetManagerRequests(manager.Jmbg, buildingCode, ManagerRequestFilter.Pending);
        if (requests.Count == 0)
        {
            Console.WriteLine("Nema zahteva na cekanju.");
            ConsoleHelper.Pause();
            return;
        }

        var rows = requests
            .Select(request => new[]
            {
                request.Id,
                request.TenantJmbg,
                request.BuildingCode,
                request.ApartmentNumber.ToString(),
                request.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
            })
            .ToList();

        TablePrinter.Print(["Id", "TenantJmbg", "Building", "Apartment", "Created"], rows);
        var requestId = InputValidator.ReadRequiredString("Unesite ID zahteva: ");

        Console.WriteLine("1. Odbij");
        Console.WriteLine("2. Potvrdi");
        var action = InputValidator.ReadMenuChoice("Izbor: ", 1, 2);

        try
        {
            if (action == 2)
            {
                managerService.ApproveRequest(manager.Jmbg, requestId);
                ConsoleHelper.PrintSuccess("Zahtev je odobren.");
            }
            else
            {
                var reason = InputValidator.ReadRequiredString("Unesite obrazlozenje odbijanja: ");
                managerService.RejectRequest(manager.Jmbg, requestId, reason);
                ConsoleHelper.PrintSuccess("Zahtev je odbijen.");
            }
        }
        catch (Exception exception)
        {
            ConsoleHelper.PrintError(exception.Message);
        }

        ConsoleHelper.Pause();
    }

    private string? SelectApprovedBuildingCode(BuildingManager manager)
    {
        var buildings = managerService.GetManagerBuildings(manager.Jmbg, ManagerBuildingFilter.Approved);
        if (buildings.Count == 0)
        {
            Console.WriteLine("Nemate odobrenih zgrada.");
            ConsoleHelper.Pause();
            return null;
        }

        var rows = buildings
            .Select(building => new[]
            {
                building.Code,
                building.Address.Street,
                building.Address.Number,
                building.Neighborhood,
                building.FloorCount.ToString()
            })
            .ToList();

        TablePrinter.Print(["Code", "Street", "Number", "Neighborhood", "Floors"], rows);

        while (true)
        {
            var buildingCode = InputValidator.ReadRequiredString("Unesite sifru zgrade: ");
            if (buildings.Any(building => string.Equals(
                building.Code,
                buildingCode,
                StringComparison.OrdinalIgnoreCase)))
            {
                return buildingCode;
            }

            ConsoleHelper.PrintError("Izabrana zgrada nije na listi vasih odobrenih zgrada.");
        }
    }

    private void ShowBuildings(BuildingManager manager)
    {
        ConsoleHelper.PrintHeader("Moje zgrade");
        Console.WriteLine("1. Na cekanju");
        Console.WriteLine("2. Prihvacene");

        var filter = InputValidator.ReadMenuChoice("Izbor: ", 1, 2) == 1
            ? ManagerBuildingFilter.Pending
            : ManagerBuildingFilter.Approved;

        var buildings = managerService.GetManagerBuildings(manager.Jmbg, filter);
        if (buildings.Count == 0)
        {
            Console.WriteLine("Nema zgrada za prikaz.");
            ConsoleHelper.Pause();
            return;
        }

        var rows = buildings
            .Select(building => new[]
            {
                building.Code,
                building.Address.Street,
                building.Address.Number,
                building.Neighborhood,
                building.FloorCount.ToString(),
                building.Status.ToString(),
                building.RejectionReason ?? string.Empty
            })
            .ToList();

        TablePrinter.Print(["Code", "Street", "Number", "Neighborhood", "Floors", "Status", "Reason"], rows);
        ConsoleHelper.Pause();
    }

    private void ProcessBuilding(BuildingManager manager)
    {
        ConsoleHelper.PrintHeader("Obrada zgrada");
        var buildings = managerService.GetManagerBuildings(manager.Jmbg, ManagerBuildingFilter.Pending);
        if (buildings.Count == 0)
        {
            Console.WriteLine("Nema zgrada na cekanju.");
            ConsoleHelper.Pause();
            return;
        }

        var rows = buildings
            .Select(building => new[]
            {
                building.Code,
                building.Address.Street,
                building.Address.Number,
                building.Neighborhood,
                building.FloorCount.ToString()
            })
            .ToList();

        TablePrinter.Print(["Code", "Street", "Number", "Neighborhood", "Floors"], rows);
        var buildingCode = InputValidator.ReadRequiredString("Unesite sifru zgrade: ");

        Console.WriteLine("1. Odbij");
        Console.WriteLine("2. Potvrdi");
        var action = InputValidator.ReadMenuChoice("Izbor: ", 1, 2);

        try
        {
            if (action == 2)
            {
                managerService.ApproveBuilding(manager.Jmbg, buildingCode);
                ConsoleHelper.PrintSuccess("Zgrada je odobrena.");
            }
            else
            {
                var reason = InputValidator.ReadOptionalString("Unesite razlog odbijanja (opciono): ");
                managerService.RejectBuilding(manager.Jmbg, buildingCode, reason);
                ConsoleHelper.PrintSuccess("Zgrada je odbijena.");
            }
        }
        catch (Exception exception)
        {
            ConsoleHelper.PrintError(exception.Message);
        }

        ConsoleHelper.Pause();
    }

    private void AddApartment(BuildingManager manager)
    {
        ConsoleHelper.PrintHeader("Unos stana");
        var apartment = new Apartment
        {
            BuildingCode = InputValidator.ReadRequiredString("Sifra zgrade: "),
            ApartmentNumber = InputValidator.ReadInt("Broj stana: ", 1),
            Description = InputValidator.ReadRequiredString("Opis stana: "),
            RoomCount = InputValidator.ReadInt("Broj soba: ", 1),
            MaxTenantCount = InputValidator.ReadInt("Max broj stanara: ", 1)
        };

        try
        {
            managerService.AddApartment(manager.Jmbg, apartment);
            ConsoleHelper.PrintSuccess("Stan je uspesno dodat.");
        }
        catch (Exception exception)
        {
            ConsoleHelper.PrintError(exception.Message);
        }

        ConsoleHelper.Pause();
    }
}
