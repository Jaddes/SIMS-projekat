using SIMSProject.Initialization;
using SIMSProject.Repositories;
using SIMSProject.RepositoryInterfaces;
using SIMSProject.ServiceInterfaces;
using SIMSProject.Services;
using System.IO;

namespace SIMSProject.Wpf;

public sealed class AppServices
{
    public AppServices()
    {
        ProjectRoot = ResolveProjectRoot();
        var storageRoot = Path.Combine(ProjectRoot, "Storage");

        Users = new UserRepository(Path.Combine(storageRoot, "users.json"));
        Buildings = new BuildingRepository(Path.Combine(storageRoot, "buildings.json"));
        Apartments = new ApartmentRepository(Path.Combine(storageRoot, "apartments.json"));
        AccessRequests = new AccessRequestRepository(Path.Combine(storageRoot, "accessRequests.json"));
        ApartmentMemberships = new ApartmentMembershipRepository(Path.Combine(storageRoot, "apartmentMemberships.json"));

        new StorageInitializer(storageRoot, Users).Initialize();

        var validationService = new ValidationService(Users, Buildings, Apartments);
        Auth = new AuthService(Users);
        SharedBuildings = new SharedBuildingService(Buildings, Apartments);
        Tenants = new TenantService(Users, AccessRequests, validationService);
        Managers = new ManagerService(Buildings, Apartments, AccessRequests, ApartmentMemberships, validationService);
        Admins = new AdminService(Users, Buildings, validationService);
    }

    public string ProjectRoot { get; }
    public IUserRepository Users { get; }
    public IBuildingRepository Buildings { get; }
    public IApartmentRepository Apartments { get; }
    public IAccessRequestRepository AccessRequests { get; }
    public IApartmentMembershipRepository ApartmentMemberships { get; }
    public IAuthService Auth { get; }
    public ISharedBuildingService SharedBuildings { get; }
    public ITenantService Tenants { get; }
    public IManagerService Managers { get; }
    public IAdminService Admins { get; }

    private static string ResolveProjectRoot()
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var directory = new DirectoryInfo(start);
            while (directory is not null)
            {
                var hasProject = File.Exists(Path.Combine(directory.FullName, "SIMSProject.csproj"));
                var hasStorage = Directory.Exists(Path.Combine(directory.FullName, "Storage"));
                if (hasProject && hasStorage)
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }
        }

        return Directory.GetCurrentDirectory();
    }
}
