using LinqToDB.Mapping;

namespace SatinRoad.Api.Models;

[Table("Orders")]
public class Order
{
    [PrimaryKey, Identity]
    public int Id { get; set; }

    [Column]
    public int BuyerId { get; set; }

    [Column]
    public int VendorId { get; set; }

    [Column]
    public decimal TotalPrice { get; set; }
}