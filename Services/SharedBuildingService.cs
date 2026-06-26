using SIMSProject.Enums;
using SIMSProject.Models;
using SIMSProject.RepositoryInterfaces;
using SIMSProject.ServiceInterfaces;

namespace SIMSProject.Services;

public class SharedBuildingService(
    IBuildingRepository buildingRepository,
    IApartmentRepository apartmentRepository) : ISharedBuildingService
{
    public List<Building> GetApprovedBuildings(bool sortByFloorCount)
    {
        var buildings = buildingRepository
            .GetAll()
            .Where(building => building.Status == BuildingStatus.Approved);

        if (sortByFloorCount)
        {
            buildings = buildings.OrderBy(building => building.FloorCount);
        }

        return buildings.ToList();
    }

    public List<Building> SearchApprovedBuildings(BuildingSearchCriteria criteria)
    {
        var approvedBuildings = GetApprovedBuildings(false);

        return criteria.Field switch
        {
            BuildingSearchField.Address => approvedBuildings
                .Where(building => ContainsIgnoreCase(
                    $"{building.Address.Street} {building.Address.Number}",
                    criteria.Query))
                .ToList(),
            BuildingSearchField.Neighborhood => approvedBuildings
                .Where(building => ContainsIgnoreCase(building.Neighborhood, criteria.Query))
                .ToList(),
            BuildingSearchField.FloorCount => SearchByFloorCount(approvedBuildings, criteria.Query),
            BuildingSearchField.ApartmentCriteria => SearchByApartmentCriteria(approvedBuildings, criteria.ApartmentCriteria),
            _ => []
        };
    }

    private List<Building> SearchByFloorCount(List<Building> buildings, string query)
    {
        if (!int.TryParse(query, out var floorCount))
        {
            return new List<Building>();
        }

        return buildings
            .Where(building => building.FloorCount == floorCount)
            .ToList();
    }

    private List<Building> SearchByApartmentCriteria(List<Building> buildings, ApartmentSearchCriteria? criteria)
    {
        if (criteria is null)
        {
            return new List<Building>();
        }

        var apartments = apartmentRepository.GetAll();

        return buildings
            .Where(building => apartments
                .Where(apartment => EqualsIgnoreCase(apartment.BuildingCode, building.Code))
                .Any(apartment => MatchesApartmentCriteria(apartment, criteria)))
            .ToList();
    }

    private static bool MatchesApartmentCriteria(Apartment apartment, ApartmentSearchCriteria criteria)
    {
        return criteria.Mode switch
        {
            ApartmentSearchMode.RoomCount => apartment.RoomCount == criteria.RoomCount,
            ApartmentSearchMode.MaxTenantCount => apartment.MaxTenantCount == criteria.MaxTenantCount,
            ApartmentSearchMode.Combined when criteria.Operator == LogicalOperator.And =>
                apartment.RoomCount == criteria.RoomCount &&
                apartment.MaxTenantCount == criteria.MaxTenantCount,
            ApartmentSearchMode.Combined when criteria.Operator == LogicalOperator.Or =>
                apartment.RoomCount == criteria.RoomCount ||
                apartment.MaxTenantCount == criteria.MaxTenantCount,
            _ => false
        };
    }

    private static bool ContainsIgnoreCase(string source, string query)
    {
        return source.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    private static bool EqualsIgnoreCase(string left, string right)
    {
        return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    }
}
