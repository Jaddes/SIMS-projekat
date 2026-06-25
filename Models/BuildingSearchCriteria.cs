using SIMSProject.Enums;

namespace SIMSProject.Models;

public class BuildingSearchCriteria
{
    public BuildingSearchField Field { get; set; }
    public string Query { get; set; } = string.Empty;
    public ApartmentSearchCriteria? ApartmentCriteria { get; set; }
}
