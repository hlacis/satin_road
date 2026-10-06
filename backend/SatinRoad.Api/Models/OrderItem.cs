using LinqToDB.Mapping;

namespace SatinRoad.Api.Models;

[Table("OrderItems")]
public class OrderItem
{
    [PrimaryKey, Identity]
    public int Id { get; set; }

    [Column]
    public int OrderId { get; set; }

    [Column]
    public int ProductId { get; set; }

    [Column]
    public int Quantity { get; set; }

    [Column]
    public decimal UnitPrice { get; set; }
}