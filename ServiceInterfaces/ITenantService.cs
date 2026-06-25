using SIMSProject.Enums;
using SIMSProject.Models;

namespace SIMSProject.ServiceInterfaces;

public interface ITenantService
{
    Tenant RegisterTenant(Tenant tenant);
    int GetActiveTenantCount(string buildingCode, int apartmentNumber);
    AccessRequest CreateAccessRequest(string tenantJmbg, string buildingCode, int apartmentNumber);
    List<AccessRequest> GetTenantRequests(string tenantJmbg, TenantRequestFilter filter);
    void WithdrawRequest(string tenantJmbg, string requestId);
}
