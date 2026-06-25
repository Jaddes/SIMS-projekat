using SIMSProject.Enums;

namespace SIMSProject.Models;

public class ApartmentSearchCriteria
{
    public ApartmentSearchMode Mode { get; set; }
    public int? RoomCount { get; set; }
    public int? MaxTenantCount { get; set; }
    public LogicalOperator? Operator { get; set; }
}
