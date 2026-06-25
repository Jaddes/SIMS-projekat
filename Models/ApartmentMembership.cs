namespace SIMSProject.Models;

public class ApartmentMembership
{
    public string Id { get; set; } = string.Empty;
    public string TenantJmbg { get; set; } = string.Empty;
    public string BuildingCode { get; set; } = string.Empty;
    public int ApartmentNumber { get; set; }
    public string ApprovedRequestId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
