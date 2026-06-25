namespace SIMSProject.RepositoryInterfaces;

public interface IRepository<T>
{
    List<T> GetAll();
    void SaveAll(List<T> entities);
}
