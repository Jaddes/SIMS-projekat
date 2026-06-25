using SIMSProject.Models;
using SIMSProject.RepositoryInterfaces;

namespace SIMSProject.Repositories;

public class ApartmentRepository(string filePath) : JsonRepositoryBase<Apartment>(filePath), IApartmentRepository
{
}
