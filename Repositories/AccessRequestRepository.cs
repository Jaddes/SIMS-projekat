using SIMSProject.Models;
using SIMSProject.RepositoryInterfaces;

namespace SIMSProject.Repositories;

public class AccessRequestRepository(string filePath) : JsonRepositoryBase<AccessRequest>(filePath), IAccessRequestRepository
{
}
