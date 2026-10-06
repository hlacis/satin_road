namespace SatinRoad.Api.Models;

public class PurchaseRequest
{
    public int BuyerId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}