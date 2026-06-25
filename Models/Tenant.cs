using SIMSProject.Enums;

namespace SIMSProject.Models;

public class Tenant : User
{
    public Tenant()
    {
        UserType = UserType.Tenant;
    }
}
