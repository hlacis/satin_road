using LinqToDB.Mapping;

namespace SatinRoad.Api.Models;


[Table("Products")]
public class Product
{
    [PrimaryKey, Identity]
    public int Id { get; set; }
    [Column]
    public string Name { get; set; } = string.Empty;
    [Column]
    public string Description { get; set; } = string.Empty;
    [Column]
    public decimal Price { get; set; }
    [Column]
    public int Stock { get; set; }
    [Column]
    public int VendorId { get; set; }
    [Column]
    public int CategoryId { get; set; }
    [Column]
    public string Condition { get; set; } = string.Empty;
    [Column]
    public bool IsActive { get; set; }
    
}