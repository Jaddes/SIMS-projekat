using SIMSProject.Enums;

namespace SIMSProject.Models;

public class Administrator : User
{
    public Administrator()
    {
        UserType = UserType.Administrator;
    }
}
