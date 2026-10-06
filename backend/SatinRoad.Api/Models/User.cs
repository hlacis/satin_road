using LinqToDB.Mapping;

namespace SatinRoad.Api.Models;

[Table("Users")]
public class User
{
    [PrimaryKey, Identity]
    public int Id { get; set; }

    [Column]
    public string Name { get; set; } = string.Empty;

    [Column]
    public string Password { get; set; } = string.Empty;

    [Column]
    public bool IsShutDown { get; set; }
}