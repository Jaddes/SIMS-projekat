using SIMSProject.Enums;

namespace SIMSProject.Models;

public class AccessRequest
{
    public string Id { get; set; } = string.Empty;
    public string TenantJmbg { get; set; } = string.Empty;
    public string BuildingCode { get; set; } = string.Empty;
    public int ApartmentNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public RequestStatus Status { get; set; } = RequestStatus.PendingApproval;
    public string? RejectionReason { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedByManagerJmbg { get; set; }
}
