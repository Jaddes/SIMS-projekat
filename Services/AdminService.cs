using SIMSProject.Enums;
using SIMSProject.Models;
using SIMSProject.RepositoryInterfaces;
using SIMSProject.ServiceInterfaces;

namespace SIMSProject.Services;

public class AdminService(
    IUserRepository userRepository,
    IBuildingRepository buildingRepository,
    ValidationService validationService) : IAdminService
{
    public BuildingManager RegisterManager(BuildingManager manager)
    {
        validationService.EnsureUserJmbgUnique(manager.Jmbg);
        validationService.EnsureUserEmailUnique(manager.Email);

        manager.UserType = UserType.BuildingManager;
        manager.CreatedAt = DateTime.UtcNow;

        var users = userRepository.GetAll();
        users.Add(manager);
        userRepository.SaveAll(users);

        return manager;
    }

    public Building CreateBuilding(Building building)
    {
        validationService.EnsureBuildingCodeUnique(building.Code);
        validationService.GetManagerByJmbg(building.ManagerJmbg);

        building.Status = BuildingStatus.PendingApproval;
        building.CreatedAt = DateTime.UtcNow;
        building.RejectionReason = null;

        var buildings = buildingRepository.GetAll();
        buildings.Add(building);
        buildingRepository.SaveAll(buildings);

        return building;
    }
}
