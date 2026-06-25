using SIMSProject.Models;

namespace SIMSProject.ServiceInterfaces;

public interface ISharedBuildingService
{
    List<Building> GetApprovedBuildings(bool sortByFloorCount);
    List<Building> SearchApprovedBuildings(BuildingSearchCriteria criteria);
}
