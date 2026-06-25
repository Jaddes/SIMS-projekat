using System.Text.Json.Serialization;
using SIMSProject.Enums;

namespace SIMSProject.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Administrator), "administrator")]
[JsonDerivedType(typeof(Tenant), "tenant")]
[JsonDerivedType(typeof(BuildingManager), "buildingManager")]
public abstract class User
{
    public string Jmbg { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MobilePhone { get; set; } = string.Empty;
    public UserType UserType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
