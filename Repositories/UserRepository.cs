using SIMSProject.Models;
using SIMSProject.RepositoryInterfaces;

namespace SIMSProject.Repositories;

public class UserRepository(string filePath) : JsonRepositoryBase<User>(filePath), IUserRepository
{
}
