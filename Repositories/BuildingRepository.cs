using SIMSProject.Models;
using SIMSProject.RepositoryInterfaces;

namespace SIMSProject.Repositories;

public class BuildingRepository(string filePath) : JsonRepositoryBase<Building>(filePath), IBuildingRepository
{
}
