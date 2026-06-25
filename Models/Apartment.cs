namespace SIMSProject.Models;

public class Apartment
{
    public string BuildingCode { get; set; } = string.Empty;
    public int ApartmentNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public int RoomCount { get; set; }
    public int MaxTenantCount { get; set; }
}
