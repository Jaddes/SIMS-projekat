using SIMSProject.Enums;
using SIMSProject.Models;
using SIMSProject.RepositoryInterfaces;
using SIMSProject.ServiceInterfaces;

namespace SIMSProject.Services;

public class ManagerService(
    IBuildingRepository buildingRepository,
    IApartmentRepository apartmentRepository,
    IAccessRequestRepository accessRequestRepository,
    IApartmentMembershipRepository apartmentMembershipRepository,
    ValidationService validationService) : IManagerService
{
    public List<AccessRequest> GetManagerRequests(string managerJmbg, string buildingCode, ManagerRequestFilter filter)
    {
        validationService.GetManagerByJmbg(managerJmbg);
        EnsureApprovedManagerBuilding(managerJmbg, buildingCode);

        var requests = accessRequestRepository
            .GetAll()
            .Where(request => EqualsIgnoreCase(request.BuildingCode, buildingCode))
            .Where(request => filter == ManagerRequestFilter.Pending
                ? request.Status == RequestStatus.PendingApproval
                : request.Status == RequestStatus.Approved)
            .OrderByDescending(request => request.CreatedAt)
            .ToList();

        return requests;
    }

    public void ApproveRequest(string managerJmbg, string requestId)
    {
        validationService.GetManagerByJmbg(managerJmbg);

        var requests = accessRequestRepository.GetAll();
        var request = requests.FirstOrDefault(item => EqualsIgnoreCase(item.Id, requestId));
        if (request is null)
        {
            throw new InvalidOperationException("Zahtev nije pronadjen.");
        }

        var building = validationService.GetApprovedBuildingByCode(request.BuildingCode);
        if (!EqualsIgnoreCase(building.ManagerJmbg, managerJmbg))
        {
            throw new InvalidOperationException("Izabrani zahtev ne pripada ovom upravniku.");
        }

        if (request.Status != RequestStatus.PendingApproval)
        {
            throw new InvalidOperationException("Mogu se obradjivati samo zahtevi na cekanju.");
        }

        var apartment = validationService.GetApartment(request.BuildingCode, request.ApartmentNumber);
        var memberships = apartmentMembershipRepository.GetAll();
        var activeMemberships = memberships
            .Where(item =>
                item.IsActive &&
                EqualsIgnoreCase(item.BuildingCode, request.BuildingCode) &&
                item.ApartmentNumber == request.ApartmentNumber)
            .ToList();

        if (activeMemberships.Any(item => EqualsIgnoreCase(item.TenantJmbg, request.TenantJmbg)))
        {
            throw new InvalidOperationException("Stanar je vec aktivno povezan sa izabranim stanom.");
        }

        if (activeMemberships.Count >= apartment.MaxTenantCount)
        {
            throw new InvalidOperationException("Stan je popunjen i zahtev ne moze biti odobren.");
        }

        request.Status = RequestStatus.Approved;
        request.RejectionReason = null;
        request.ResolvedAt = DateTime.UtcNow;
        request.ResolvedByManagerJmbg = managerJmbg;
        accessRequestRepository.SaveAll(requests);

        memberships.Add(new ApartmentMembership
        {
            Id = CreateId(),
            TenantJmbg = request.TenantJmbg,
            BuildingCode = request.BuildingCode,
            ApartmentNumber = request.ApartmentNumber,
            ApprovedRequestId = request.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        apartmentMembershipRepository.SaveAll(memberships);
    }

    public void RejectRequest(string managerJmbg, string requestId, string rejectionReason)
    {
        validationService.GetManagerByJmbg(managerJmbg);

        if (string.IsNullOrWhiteSpace(rejectionReason))
        {
            throw new InvalidOperationException("Obrazlozenje odbijanja je obavezno.");
        }

        var requests = accessRequestRepository.GetAll();
        var request = requests.FirstOrDefault(item => EqualsIgnoreCase(item.Id, requestId));
        if (request is null)
        {
            throw new InvalidOperationException("Zahtev nije pronadjen.");
        }

        var building = validationService.GetApprovedBuildingByCode(request.BuildingCode);
        if (!EqualsIgnoreCase(building.ManagerJmbg, managerJmbg))
        {
            throw new InvalidOperationException("Izabrani zahtev ne pripada ovom upravniku.");
        }

        if (request.Status != RequestStatus.PendingApproval)
        {
            throw new InvalidOperationException("Mogu se obradjivati samo zahtevi na cekanju.");
        }

        request.Status = RequestStatus.Rejected;
        request.RejectionReason = rejectionReason.Trim();
        request.ResolvedAt = DateTime.UtcNow;
        request.ResolvedByManagerJmbg = managerJmbg;
        accessRequestRepository.SaveAll(requests);
    }

    public List<Building> GetManagerBuildings(string managerJmbg, ManagerBuildingFilter filter)
    {
        validationService.GetManagerByJmbg(managerJmbg);

        return buildingRepository
            .GetAll()
            .Where(building => EqualsIgnoreCase(building.ManagerJmbg, managerJmbg))
            .Where(building => filter == ManagerBuildingFilter.Pending
                ? building.Status == BuildingStatus.PendingApproval
                : building.Status == BuildingStatus.Approved)
            .OrderBy(building => building.Code)
            .ToList();
    }

    public void ApproveBuilding(string managerJmbg, string buildingCode)
    {
        validationService.GetManagerByJmbg(managerJmbg);

        var buildings = buildingRepository.GetAll();
        var building = buildings.FirstOrDefault(item => EqualsIgnoreCase(item.Code, buildingCode));
        if (building is null)
        {
            throw new InvalidOperationException("Zgrada nije pronadjena.");
        }

        if (!EqualsIgnoreCase(building.ManagerJmbg, managerJmbg))
        {
            throw new InvalidOperationException("Izabrana zgrada ne pripada ovom upravniku.");
        }

        if (building.Status != BuildingStatus.PendingApproval)
        {
            throw new InvalidOperationException("Mogu se odobravati samo zgrade na cekanju.");
        }

        building.Status = BuildingStatus.Approved;
        building.RejectionReason = null;
        buildingRepository.SaveAll(buildings);
    }

    public void RejectBuilding(string managerJmbg, string buildingCode, string? rejectionReason)
    {
        validationService.GetManagerByJmbg(managerJmbg);

        var buildings = buildingRepository.GetAll();
        var building = buildings.FirstOrDefault(item => EqualsIgnoreCase(item.Code, buildingCode));
        if (building is null)
        {
            throw new InvalidOperationException("Zgrada nije pronadjena.");
        }

        if (!EqualsIgnoreCase(building.ManagerJmbg, managerJmbg))
        {
            throw new InvalidOperationException("Izabrana zgrada ne pripada ovom upravniku.");
        }

        if (building.Status != BuildingStatus.PendingApproval)
        {
            throw new InvalidOperationException("Mogu se odbijati samo zgrade na cekanju.");
        }

        building.Status = BuildingStatus.Rejected;
        building.RejectionReason = string.IsNullOrWhiteSpace(rejectionReason) ? null : rejectionReason.Trim();
        buildingRepository.SaveAll(buildings);
    }

    public Apartment AddApartment(string managerJmbg, Apartment apartment)
    {
        validationService.GetManagerByJmbg(managerJmbg);
        validationService.EnsureApartmentUnique(apartment.BuildingCode, apartment.ApartmentNumber);

        var building = validationService.GetApprovedBuildingByCode(apartment.BuildingCode);
        if (!EqualsIgnoreCase(building.ManagerJmbg, managerJmbg))
        {
            throw new InvalidOperationException("Upravnik moze unositi stanove samo u svoje zgrade.");
        }

        var apartments = apartmentRepository.GetAll();
        apartments.Add(apartment);
        apartmentRepository.SaveAll(apartments);

        return apartment;
    }

    private void EnsureApprovedManagerBuilding(string managerJmbg, string buildingCode)
    {
        var building = validationService.GetApprovedBuildingByCode(buildingCode);
        if (!EqualsIgnoreCase(building.ManagerJmbg, managerJmbg))
        {
            throw new InvalidOperationException("Izabrana zgrada ne pripada ovom upravniku.");
        }
    }

    private static string CreateId()
    {
        return Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    }

    private static bool EqualsIgnoreCase(string left, string right)
    {
        return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    }
}
