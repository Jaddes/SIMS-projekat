using SIMSProject.Enums;

namespace SIMSProject.Models;

public class Building
{
    public string Code { get; set; } = string.Empty;
    public Address Address { get; set; } = new();
    public string Neighborhood { get; set; } = string.Empty;
    public Location Location { get; set; } = new();
    public int FloorCount { get; set; }
    public string ManagerJmbg { get; set; } = string.Empty;
    public BuildingStatus Status { get; set; } = BuildingStatus.PendingApproval;
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
