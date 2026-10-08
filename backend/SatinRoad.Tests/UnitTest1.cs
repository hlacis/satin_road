using NUnit.Framework;
using SatinRoad.Api.Models;
using SatinRoad.Api.Results;
using SatinRoad.Api.Services;

namespace SatinRoad.Tests;

public class ProductServiceTests
{
    // A product without a name should be rejected
    [Test]
    public async Task CreateProduct_WithEmptyName_ReturnsInvalid()
    {
        var service = new ProductService(null!);

        var product = new Product
        {
            Name = "",
            Price = 50,
            Stock = 10,
            VendorId = 3,
            CategoryId = 1
        };

        var result = await service.CreateProduct(product);

        Assert.That(result.Result, Is.EqualTo(ProductResult.Invalid));
        Assert.That(result.Product, Is.Null);
    }
    
    //A product with a negative price should be rejected
    [Test]
    public async Task CreateProduct_WithNegativePrice_ReturnsInvalid()
    {
        var service = new ProductService(null!);

        var product = new Product
        {
            Name = "Test Product",
            Price = -10,
            Stock = 10,
            VendorId = 3,
            CategoryId = 1
        };

        var result = await service.CreateProduct(product);

        Assert.That(result.Result, Is.EqualTo(ProductResult.Invalid));
        Assert.That(result.Product, Is.Null);
    }
    
    // A product with negative stock should be rejected
    [Test]
    public async Task CreateProduct_WithNegativeStock_ReturnsInvalid()
    {
        var service = new ProductService(null!);

        var product = new Product
        {
            Name = "Test Product",
            Price = 50,
            Stock = -1,
            VendorId = 3,
            CategoryId = 1
        };

        var result = await service.CreateProduct(product);

        Assert.That(result.Result, Is.EqualTo(ProductResult.Invalid));
        Assert.That(result.Product, Is.Null);
    }
}