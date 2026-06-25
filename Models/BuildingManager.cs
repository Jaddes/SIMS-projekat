using SIMSProject.Enums;

namespace SIMSProject.Models;

public class BuildingManager : User
{
    public BuildingManager()
    {
        UserType = UserType.BuildingManager;
    }
}
