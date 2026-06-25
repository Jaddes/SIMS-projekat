using SIMSProject.Enums;
using SIMSProject.Models;
using SIMSProject.RepositoryInterfaces;

namespace SIMSProject.Services;

public class ValidationService(
    IUserRepository userRepository,
    IBuildingRepository buildingRepository,
    IApartmentRepository apartmentRepository)
{
    public void EnsureUserJmbgUnique(string jmbg)
    {
        var exists = userRepository
            .GetAll()
            .Any(user => EqualsIgnoreCase(user.Jmbg, jmbg));

        if (exists)
        {
            throw new InvalidOperationException("Korisnik sa unetim JMBG vec postoji.");
        }
    }

    public void EnsureUserEmailUnique(string email)
    {
        var exists = userRepository
            .GetAll()
            .Any(user => EqualsIgnoreCase(user.Email, email));

        if (exists)
        {
            throw new InvalidOperationException("Korisnik sa unetim email-om vec postoji.");
        }
    }

    public void EnsureTenantPasswordUnique(string password)
    {
        var exists = userRepository
            .GetAll()
            .Any(user => user.Password == password);

        if (exists)
        {
            throw new InvalidOperationException("Lozinka mora biti jedinstvena za registraciju stanara.");
        }
    }

    public void EnsureBuildingCodeUnique(string buildingCode)
    {
        var exists = buildingRepository
            .GetAll()
            .Any(building => EqualsIgnoreCase(building.Code, buildingCode));

        if (exists)
        {
            throw new InvalidOperationException("Zgrada sa unetom sifrom vec postoji.");
        }
    }

    public void EnsureApartmentUnique(string buildingCode, int apartmentNumber)
    {
        var exists = apartmentRepository
            .GetAll()
            .Any(apartment =>
                EqualsIgnoreCase(apartment.BuildingCode, buildingCode) &&
                apartment.ApartmentNumber == apartmentNumber);

        if (exists)
        {
            throw new InvalidOperationException("Stan sa unetim brojem vec postoji u izabranoj zgradi.");
        }
    }

    public BuildingManager GetManagerByJmbg(string managerJmbg)
    {
        var manager = userRepository
            .GetAll()
            .OfType<BuildingManager>()
            .FirstOrDefault(user => EqualsIgnoreCase(user.Jmbg, managerJmbg));

        return manager ?? throw new InvalidOperationException("Upravnik sa unetim JMBG ne postoji.");
    }

    public Tenant GetTenantByJmbg(string tenantJmbg)
    {
        var tenant = userRepository
            .GetAll()
            .OfType<Tenant>()
            .FirstOrDefault(user => EqualsIgnoreCase(user.Jmbg, tenantJmbg));

        return tenant ?? throw new InvalidOperationException("Stanar sa unetim JMBG ne postoji.");
    }

    public Building GetBuildingByCode(string buildingCode)
    {
        var building = buildingRepository
            .GetAll()
            .FirstOrDefault(item => EqualsIgnoreCase(item.Code, buildingCode));

        return building ?? throw new InvalidOperationException("Zgrada sa unetom sifrom ne postoji.");
    }

    public Building GetApprovedBuildingByCode(string buildingCode)
    {
        var building = GetBuildingByCode(buildingCode);
        if (building.Status != BuildingStatus.Approved)
        {
            throw new InvalidOperationException("Izabrana zgrada nije odobrena.");
        }

        return building;
    }

    public Apartment GetApartment(string buildingCode, int apartmentNumber)
    {
        var apartment = apartmentRepository
            .GetAll()
            .FirstOrDefault(item =>
                EqualsIgnoreCase(item.BuildingCode, buildingCode) &&
                item.ApartmentNumber == apartmentNumber);

        return apartment ?? throw new InvalidOperationException("Stan sa unetim brojem ne postoji u izabranoj zgradi.");
    }

    private static bool EqualsIgnoreCase(string left, string right)
    {
        return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    }
}
