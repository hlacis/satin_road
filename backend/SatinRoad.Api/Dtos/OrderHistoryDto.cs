namespace SatinRoad.Api.Dtos;

public class OrderHistoryDto
{
    public int OrderId { get; set; }
    public int VendorId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal OriginalUnitPrice { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}