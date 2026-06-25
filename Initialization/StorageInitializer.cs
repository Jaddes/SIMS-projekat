using SIMSProject.Models;
using SIMSProject.RepositoryInterfaces;

namespace SIMSProject.Initialization;

public class StorageInitializer(string storageRoot, IUserRepository userRepository)
{
    public void Initialize()
    {
        Directory.CreateDirectory(storageRoot);

        EnsureFile("users.json");
        EnsureFile("buildings.json");
        EnsureFile("apartments.json");
        EnsureFile("accessRequests.json");
        EnsureFile("apartmentMemberships.json");

        SeedAdministrator();
    }

    private void EnsureFile(string fileName)
    {
        var filePath = Path.Combine(storageRoot, fileName);
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "[]");
        }
    }

    private void SeedAdministrator()
    {
        var users = userRepository.GetAll();
        var hasAdministrator = users.OfType<Administrator>().Any();
        if (hasAdministrator)
        {
            return;
        }

        users.Add(new Administrator
        {
            Jmbg = "0000000000000",
            Email = "admin@sims.local",
            Password = "Admin123!",
            FirstName = "System",
            LastName = "Administrator",
            MobilePhone = "0600000000",
            CreatedAt = DateTime.UtcNow
        });

        userRepository.SaveAll(users);
    }
}
