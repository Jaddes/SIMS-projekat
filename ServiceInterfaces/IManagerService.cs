using SIMSProject.Enums;
using SIMSProject.Models;

namespace SIMSProject.ServiceInterfaces;

public interface IManagerService
{
    List<AccessRequest> GetManagerRequests(string managerJmbg, string buildingCode, ManagerRequestFilter filter);
    void ApproveRequest(string managerJmbg, string requestId);
    void RejectRequest(string managerJmbg, string requestId, string rejectionReason);
    List<Building> GetManagerBuildings(string managerJmbg, ManagerBuildingFilter filter);
    void ApproveBuilding(string managerJmbg, string buildingCode);
    void RejectBuilding(string managerJmbg, string buildingCode, string? rejectionReason);
    Apartment AddApartment(string managerJmbg, Apartment apartment);
}
