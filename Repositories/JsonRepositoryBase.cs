using System.Text.Json;
using System.Text.Json.Serialization;
using SIMSProject.RepositoryInterfaces;

namespace SIMSProject.Repositories;

public abstract class JsonRepositoryBase<T> : IRepository<T>
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    protected JsonRepositoryBase(string filePath)
    {
        _filePath = filePath;
        EnsureStorage();
    }

    public List<T> GetAll()
    {
        EnsureStorage();

        var json = File.ReadAllText(_filePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? [];
    }

    public void SaveAll(List<T> entities)
    {
        EnsureStorage();
        var json = JsonSerializer.Serialize(entities, _jsonOptions);
        File.WriteAllText(_filePath, json);
    }

    private void EnsureStorage()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, "[]");
        }
    }
}
