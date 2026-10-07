using LinqToDB.Mapping;

namespace SatinRoad.Api.Models;

[Table("Categories")]
public class Category
{
    [Column("Id"), Identity]
    public int Id { get; set; }

    [Column("Name")]
    public string Name { get; set; } = string.Empty;
}