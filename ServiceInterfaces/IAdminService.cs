using SIMSProject.Models;

namespace SIMSProject.ServiceInterfaces;

public interface IAdminService
{
    BuildingManager RegisterManager(BuildingManager manager);
    Building CreateBuilding(Building building);
}
