using LinqToDB;
using LinqToDB.Async;
using SatinRoad.Api.Data;
using SatinRoad.Api.Models;
using SatinRoad.Api.Results;

namespace SatinRoad.Api.Services;

// Handles product related database operations
public class ProductService
{
    private readonly SatinRoadDb _db;

    public ProductService(SatinRoadDb db)
    {
        _db = db;
    }
    public async Task<List<Product>> GetAllProducts()
    {
        return await _db.Products
            .Where(p => p.IsActive)
            .OrderByDescending(p => _db.Orders.Count(o => o.VendorId == p.VendorId) > 100)
            .ToListAsync();
    }
    public async Task<Product?> GetProductById(int id)
    {
        return await _db.Products
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    public async Task<(ProductResult Result, Product? Product)> CreateProduct(Product product)
    {
        // Validate basic product data
        if (string.IsNullOrWhiteSpace(product.Name) ||
            product.Price < 0 ||
            product.Stock < 0)
        {
            return (ProductResult.Invalid, null);
        }

        var vendor = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == product.VendorId);

        if (vendor == null || vendor.IsShutDown)
        {
            return (ProductResult.VendorNotFound, null);
        
        }
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == product.CategoryId);

        if (category == null)
        {
            return (ProductResult.CategoryNotFound, null);
        }

        product.Id = await _db.InsertWithInt32IdentityAsync(product);

        return (ProductResult.Success, product);
    }
    public async Task<ProductResult> UpdateProduct(int id, Product product)
    {
        // Validate basic product data
        if (string.IsNullOrWhiteSpace(product.Name) ||
            product.Price < 0 ||
            product.Stock < 0)
        {
            return ProductResult.Invalid;
        }

        var existingProduct = await _db.Products
            .FirstOrDefaultAsync(p => p.Id == id);

        if (existingProduct == null)
        {
            return ProductResult.NotFound;
        }
        var vendor = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == product.VendorId);

        if (vendor == null || vendor.IsShutDown)
        {
            return ProductResult.VendorNotFound;
        }

        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == product.CategoryId);

        if (category == null)
        {
            return ProductResult.CategoryNotFound;
        }

        var affectedRows = await _db.Products
            .Where(p => p.Id == id)
            .Set(p => p.Name, product.Name)
            .Set(p => p.Description, product.Description)
            .Set(p => p.Price, product.Price)
            .Set(p => p.Stock, product.Stock)
            .Set(p => p.VendorId, product.VendorId)
            .Set(p => p.CategoryId, product.CategoryId)
            .Set(p => p.Condition, product.Condition)
            .Set(p => p.ImageUrl, product.ImageUrl)
            .UpdateAsync();

        return affectedRows > 0
            ? ProductResult.Success
            : ProductResult.NotFound;
    }
    public async Task<bool> DeleteProduct(int id)
    {
        var affectedRows = await _db.Products
            .Where(p => p.Id == id)
            .DeleteAsync();

        return affectedRows > 0;
    }
}