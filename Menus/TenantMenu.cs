using SIMSProject.Enums;
using SIMSProject.Models;
using SIMSProject.ServiceInterfaces;
using SIMSProject.Utils;

namespace SIMSProject.Menus;

public class TenantMenu(
    SharedMenu sharedMenu,
    ITenantService tenantService)
{
    public void Show(Tenant tenant)
    {
        var running = true;

        while (running)
        {
            ConsoleHelper.PrintHeader($"Tenant meni - {tenant.FirstName} {tenant.LastName}");
            Console.WriteLine("1. Prikaz odobrenih zgrada");
            Console.WriteLine("2. Pretraga zgrada");
            Console.WriteLine("3. Podnosenje zahteva za pristup");
            Console.WriteLine("4. Prikaz mojih zahteva");
            Console.WriteLine("5. Povlacenje zahteva");
            Console.WriteLine("0. Odjava");

            switch (InputValidator.ReadMenuChoice("Izbor: ", 0, 1, 2, 3, 4, 5))
            {
                case 1:
                    sharedMenu.ShowApprovedBuildings();
                    break;
                case 2:
                    sharedMenu.SearchBuildings();
                    break;
                case 3:
                    CreateAccessRequest(tenant);
                    break;
                case 4:
                    ShowRequests(tenant);
                    break;
                case 5:
                    WithdrawRequest(tenant);
                    break;
                case 0:
                    running = false;
                    break;
            }
        }
    }

    private void CreateAccessRequest(Tenant tenant)
    {
        while (true)
        {
            ConsoleHelper.PrintHeader("Podnosenje zahteva");
            Console.WriteLine("Pre kreiranja zahteva proverite sifru zgrade kroz prikaz ili pretragu zgrada.");
            Console.WriteLine();

            var buildingCode = InputValidator.ReadRequiredString("Unesite sifru zgrade: ");
            var apartmentNumber = InputValidator.ReadInt("Unesite broj stana: ", 1);

            try
            {
                var activeTenantCount = tenantService.GetActiveTenantCount(buildingCode, apartmentNumber);
                if (activeTenantCount > 0)
                {
                    Console.WriteLine($"Upozorenje: stan vec ima {activeTenantCount} aktivnih stanara.");
                }

                Console.WriteLine("1. Potvrdi kreiranje zahteva");
                Console.WriteLine("2. Promeni unos");
                Console.WriteLine("0. Odustani");

                var choice = InputValidator.ReadMenuChoice("Izbor: ", 0, 1, 2);
                if (choice == 0)
                {
                    return;
                }

                if (choice == 2)
                {
                    continue;
                }

                var request = tenantService.CreateAccessRequest(tenant.Jmbg, buildingCode, apartmentNumber);
                ConsoleHelper.PrintSuccess($"Zahtev je kreiran. ID: {request.Id}");
                ConsoleHelper.Pause();
                return;
            }
            catch (Exception exception)
            {
                ConsoleHelper.PrintError(exception.Message);
                if (!InputValidator.ReadYesNo("Da li zelite da probate ponovo? (y/n): "))
                {
                    return;
                }
            }
        }
    }

    private void ShowRequests(Tenant tenant)
    {
        ConsoleHelper.PrintHeader("Moji zahtevi");
        Console.WriteLine("1. Svi");
        Console.WriteLine("2. Na cekanju");
        Console.WriteLine("3. Odobreni");
        Console.WriteLine("4. Odbijeni");

        var filter = InputValidator.ReadMenuChoice("Izbor: ", 1, 2, 3, 4) switch
        {
            2 => TenantRequestFilter.Pending,
            3 => TenantRequestFilter.Approved,
            4 => TenantRequestFilter.Rejected,
            _ => TenantRequestFilter.All
        };

        var requests = tenantService.GetTenantRequests(tenant.Jmbg, filter);
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
                request.BuildingCode,
                request.ApartmentNumber.ToString(),
                request.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                request.Status.ToString(),
                request.RejectionReason ?? string.Empty
            })
            .ToList();

        TablePrinter.Print(
            ["Id", "Building", "Apartment", "Created", "Status", "RejectionReason"],
            rows);

        ConsoleHelper.Pause();
    }

    private void WithdrawRequest(Tenant tenant)
    {
        ConsoleHelper.PrintHeader("Povlacenje zahteva");
        var requests = tenantService.GetTenantRequests(tenant.Jmbg, TenantRequestFilter.Pending);
        if (requests.Count == 0)
        {
            Console.WriteLine("Nemate zahteva na cekanju.");
            ConsoleHelper.Pause();
            return;
        }

        var rows = requests
            .Select(request => new[]
            {
                request.Id,
                request.BuildingCode,
                request.ApartmentNumber.ToString(),
                request.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
            })
            .ToList();

        TablePrinter.Print(["Id", "Building", "Apartment", "Created"], rows);
        var requestId = InputValidator.ReadRequiredString("Unesite ID zahteva za povlacenje: ");

        try
        {
            tenantService.WithdrawRequest(tenant.Jmbg, requestId);
            ConsoleHelper.PrintSuccess("Zahtev je povucen i obrisan iz sistema.");
        }
        catch (Exception exception)
        {
            ConsoleHelper.PrintError(exception.Message);
        }

        ConsoleHelper.Pause();
    }
}
