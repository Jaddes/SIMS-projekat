using SIMSProject.Models;
using SIMSProject.RepositoryInterfaces;

namespace SIMSProject.Repositories;

public class ApartmentMembershipRepository(string filePath) : JsonRepositoryBase<ApartmentMembership>(filePath), IApartmentMembershipRepository
{
}
