using SIMSProject.Initialization;
using SIMSProject.Menus;
using SIMSProject.Repositories;
using SIMSProject.RepositoryInterfaces;
using SIMSProject.ServiceInterfaces;
using SIMSProject.Services;
using SIMSProject.Utils;

var projectRoot = ResolveProjectRoot();
var storageRoot = Path.Combine(projectRoot, "Storage");

IUserRepository userRepository = new UserRepository(Path.Combine(storageRoot, "users.json"));
IBuildingRepository buildingRepository = new BuildingRepository(Path.Combine(storageRoot, "buildings.json"));
IApartmentRepository apartmentRepository = new ApartmentRepository(Path.Combine(storageRoot, "apartments.json"));
IAccessRequestRepository accessRequestRepository = new AccessRequestRepository(Path.Combine(storageRoot, "accessRequests.json"));
IApartmentMembershipRepository apartmentMembershipRepository = new ApartmentMembershipRepository(Path.Combine(storageRoot, "apartmentMemberships.json"));

var storageInitializer = new StorageInitializer(storageRoot, userRepository);
storageInitializer.Initialize();

var validationService = new ValidationService(userRepository, buildingRepository, apartmentRepository);
IAuthService authService = new AuthService(userRepository);
ISharedBuildingService sharedBuildingService = new SharedBuildingService(buildingRepository, apartmentRepository);
ITenantService tenantService = new TenantService(userRepository, accessRequestRepository, validationService);
IManagerService managerService = new ManagerService(buildingRepository, apartmentRepository, accessRequestRepository, apartmentMembershipRepository, validationService);
IAdminService adminService = new AdminService(userRepository, buildingRepository, validationService);

var sharedMenu = new SharedMenu(sharedBuildingService);
var loginMenu = new LoginMenu(authService);
var tenantMenu = new TenantMenu(sharedMenu, tenantService);
var managerMenu = new ManagerMenu(sharedMenu, managerService);
var adminMenu = new AdminMenu(sharedMenu, adminService);
var mainMenu = new MainMenu(loginMenu, tenantService, tenantMenu, managerMenu, adminMenu);

try
{
    mainMenu.Show();
}
catch (Exception exception)
{
    ConsoleHelper.PrintError($"Neocekivana greska: {exception.Message}");
    ConsoleHelper.Pause();
}

static string ResolveProjectRoot()
{
    var currentDirectory = Directory.GetCurrentDirectory();

    if (File.Exists(Path.Combine(currentDirectory, "SIMSProject.csproj")))
    {
        return currentDirectory;
    }

    var nestedProjectPath = Path.Combine(currentDirectory, "SIMSProject");
    if (Directory.Exists(nestedProjectPath) &&
        File.Exists(Path.Combine(nestedProjectPath, "SIMSProject.csproj")))
    {
        return nestedProjectPath;
    }

    return currentDirectory;
}
