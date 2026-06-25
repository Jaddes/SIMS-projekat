using SIMSProject.Enums;
using SIMSProject.Models;
using SIMSProject.RepositoryInterfaces;
using SIMSProject.ServiceInterfaces;

namespace SIMSProject.Services;

public class TenantService(
    IUserRepository userRepository,
    IAccessRequestRepository accessRequestRepository,
    IApartmentMembershipRepository apartmentMembershipRepository,
    ValidationService validationService) : ITenantService
{
    public Tenant RegisterTenant(Tenant tenant)
    {
        validationService.EnsureUserJmbgUnique(tenant.Jmbg);
        validationService.EnsureUserEmailUnique(tenant.Email);
        validationService.EnsureTenantPasswordUnique(tenant.Password);

        tenant.UserType = UserType.Tenant;
        tenant.CreatedAt = DateTime.UtcNow;

        var users = userRepository.GetAll();
        users.Add(tenant);
        userRepository.SaveAll(users);

        return tenant;
    }

    public int GetActiveTenantCount(string buildingCode, int apartmentNumber)
    {
        validationService.GetApprovedBuildingByCode(buildingCode);
        validationService.GetApartment(buildingCode, apartmentNumber);

        return apartmentMembershipRepository
            .GetAll()
            .Count(item =>
                item.IsActive &&
                EqualsIgnoreCase(item.BuildingCode, buildingCode) &&
                item.ApartmentNumber == apartmentNumber);
    }

    public AccessRequest CreateAccessRequest(string tenantJmbg, string buildingCode, int apartmentNumber)
    {
        validationService.GetTenantByJmbg(tenantJmbg);
        validationService.GetApprovedBuildingByCode(buildingCode);
        validationService.GetApartment(buildingCode, apartmentNumber);

        var requests = accessRequestRepository.GetAll();
        var request = new AccessRequest
        {
            Id = CreateId(),
            TenantJmbg = tenantJmbg,
            BuildingCode = buildingCode,
            ApartmentNumber = apartmentNumber,
            CreatedAt = DateTime.UtcNow,
            Status = RequestStatus.PendingApproval
        };

        requests.Add(request);
        accessRequestRepository.SaveAll(requests);

        return request;
    }

    public List<AccessRequest> GetTenantRequests(string tenantJmbg, TenantRequestFilter filter)
    {
        validationService.GetTenantByJmbg(tenantJmbg);

        var requests = accessRequestRepository
            .GetAll()
            .Where(request => EqualsIgnoreCase(request.TenantJmbg, tenantJmbg));

        requests = filter switch
        {
            TenantRequestFilter.Pending => requests.Where(request => request.Status == RequestStatus.PendingApproval),
            TenantRequestFilter.Approved => requests.Where(request => request.Status == RequestStatus.Approved),
            TenantRequestFilter.Rejected => requests.Where(request => request.Status == RequestStatus.Rejected),
            _ => requests
        };

        return requests
            .OrderByDescending(request => request.CreatedAt)
            .ToList();
    }

    public void WithdrawRequest(string tenantJmbg, string requestId)
    {
        var requests = accessRequestRepository.GetAll();
        var request = requests.FirstOrDefault(item =>
            EqualsIgnoreCase(item.Id, requestId) &&
            EqualsIgnoreCase(item.TenantJmbg, tenantJmbg));

        if (request is null)
        {
            throw new InvalidOperationException("Zahtev nije pronadjen.");
        }

        if (request.Status != RequestStatus.PendingApproval)
        {
            throw new InvalidOperationException("Moguce je povuci samo zahtev koji je na cekanju.");
        }

        requests.Remove(request);
        accessRequestRepository.SaveAll(requests);
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
