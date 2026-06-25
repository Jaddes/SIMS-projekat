using SIMSProject.Enums;
using SIMSProject.Models;
using SIMSProject.ServiceInterfaces;
using SIMSProject.Utils;

namespace SIMSProject.Menus;

public class SharedMenu(ISharedBuildingService sharedBuildingService)
{
    public void ShowApprovedBuildings()
    {
        ConsoleHelper.PrintHeader("Prikaz odobrenih zgrada");

        var sortByFloorCount = InputValidator.ReadYesNo("Sortirati po broju spratova? (y/n): ");
        var buildings = sharedBuildingService.GetApprovedBuildings(sortByFloorCount);
        PrintBuildings(buildings);
        ConsoleHelper.Pause();
    }

    public void SearchBuildings()
    {
        ConsoleHelper.PrintHeader("Pretraga zgrada");
        Console.WriteLine("1. Po adresi");
        Console.WriteLine("2. Po naselju");
        Console.WriteLine("3. Po broju spratova");
        Console.WriteLine("4. Po stanovima");
        Console.WriteLine("0. Nazad");

        var choice = InputValidator.ReadMenuChoice("Izbor: ", 0, 1, 2, 3, 4);
        if (choice == 0)
        {
            return;
        }

        var criteria = choice switch
        {
            1 => new BuildingSearchCriteria
            {
                Field = BuildingSearchField.Address,
                Query = InputValidator.ReadRequiredString("Unesite deo adrese: ")
            },
            2 => new BuildingSearchCriteria
            {
                Field = BuildingSearchField.Neighborhood,
                Query = InputValidator.ReadRequiredString("Unesite deo naziva naselja: ")
            },
            3 => new BuildingSearchCriteria
            {
                Field = BuildingSearchField.FloorCount,
                Query = InputValidator.ReadRequiredString("Unesite broj spratova: ")
            },
            4 => new BuildingSearchCriteria
            {
                Field = BuildingSearchField.ApartmentCriteria,
                ApartmentCriteria = ReadApartmentCriteria()
            },
            _ => null
        };

        if (criteria is null)
        {
            return;
        }

        var buildings = sharedBuildingService.SearchApprovedBuildings(criteria);
        PrintBuildings(buildings);
        ConsoleHelper.Pause();
    }

    public static void PrintBuildings(List<Building> buildings)
    {
        if (buildings.Count == 0)
        {
            Console.WriteLine("Nema rezultata.");
            return;
        }

        var rows = buildings
            .Select(building => new[]
            {
                building.Code,
                building.Address.Street,
                building.Address.Number,
                building.Neighborhood,
                building.Location.City,
                building.Location.Country,
                building.FloorCount.ToString()
            })
            .ToList();

        TablePrinter.Print(
            ["Code", "Street", "Number", "Neighborhood", "City", "Country", "Floors"],
            rows);
    }

    private static ApartmentSearchCriteria ReadApartmentCriteria()
    {
        Console.WriteLine("1. Po broju soba");
        Console.WriteLine("2. Po max broju stanara");
        Console.WriteLine("3. Broj soba + broj stanara");

        var choice = InputValidator.ReadMenuChoice("Izbor: ", 1, 2, 3);
        return choice switch
        {
            1 => new ApartmentSearchCriteria
            {
                Mode = ApartmentSearchMode.RoomCount,
                RoomCount = InputValidator.ReadInt("Unesite broj soba: ", 1)
            },
            2 => new ApartmentSearchCriteria
            {
                Mode = ApartmentSearchMode.MaxTenantCount,
                MaxTenantCount = InputValidator.ReadInt("Unesite max broj stanara: ", 1)
            },
            _ => ReadCombinedApartmentCriteria()
        };
    }

    private static ApartmentSearchCriteria ReadCombinedApartmentCriteria()
    {
        while (true)
        {
            var input = InputValidator.ReadRequiredString("Unesite kriterijum u formatu 2 & 3 ili 2 | 3: ");
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 3 &&
                int.TryParse(parts[0], out var roomCount) &&
                int.TryParse(parts[2], out var maxTenantCount) &&
                (parts[1] == "&" || parts[1] == "|"))
            {
                return new ApartmentSearchCriteria
                {
                    Mode = ApartmentSearchMode.Combined,
                    RoomCount = roomCount,
                    MaxTenantCount = maxTenantCount,
                    Operator = parts[1] == "&" ? LogicalOperator.And : LogicalOperator.Or
                };
            }

            ConsoleHelper.PrintError("Format mora biti kao primer: 2 & 3");
        }
    }
}
