using LinqToDB;
using LinqToDB.Async;
using SatinRoad.Api.Data;
using SatinRoad.Api.Models;

namespace SatinRoad.Api.Services;

// Handles purchase related database operations
public class PurchaseService
{
    private readonly SatinRoadDb _db;
    public PurchaseService(SatinRoadDb db)
    {
        _db = db;
    }
    public async Task<Product?> GetAvailableProduct(int productId, int quantity)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null || !product.IsActive)
        {
            return null;
        }

        if (quantity <= 0 || product.Stock < quantity)
        {
            return null;
        }

        return product;
    }
    public async Task<bool> CheckFbiPurchase(Product product)
    {
        var isFbiPurchase = Random.Shared.Next(100) == 0;

        if (!isFbiPurchase)
        {
            return false;
        }

        await _db.Users
            .Where(u => u.Id == product.VendorId)
            .Set(u => u.IsShutDown, true)
            .UpdateAsync();

        await _db.Products
            .Where(p => p.VendorId == product.VendorId)
            .Set(p => p.IsActive, false)
            .UpdateAsync();

        return true;
    }
    public async Task<decimal> CalculateTotalPrice(PurchaseRequest request, Product product)
    {
        var previousOrderCount = await _db.Orders
            .CountAsync(o => o.BuyerId == request.BuyerId &&
                             o.VendorId == product.VendorId);

        var totalPrice = product.Price * request.Quantity;

        if (previousOrderCount > 10)
        {
            totalPrice = Math.Round(totalPrice * 0.8m, 2);
        }

        return totalPrice;
    }
    public async Task<Order> CreateOrder(PurchaseRequest request, Product product, decimal totalPrice)
    {
        var order = new Order
        {
            BuyerId = request.BuyerId,
            VendorId = product.VendorId,
            TotalPrice = totalPrice
        };

        order.Id = await _db.InsertWithInt32IdentityAsync(order);

        var orderItem = new OrderItem
        {
            OrderId = order.Id,
            ProductId = product.Id,
            Quantity = request.Quantity,
            UnitPrice = Math.Round(totalPrice / request.Quantity, 2)
        };

        await _db.InsertAsync(orderItem);

        return order;
    }
    public async Task UpdateStock(Product product, int quantity)
    {
        product.Stock -= quantity;

        await _db.Products
            .Where(p => p.Id == product.Id)
            .Set(p => p.Stock, product.Stock)
            .UpdateAsync();
    }
}