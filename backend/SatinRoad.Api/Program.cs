using LinqToDB;
using LinqToDB.Async;
using SatinRoad.Api.Data;
using SatinRoad.Api.Models;
using SatinRoad.Api.Services;



var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SatinRoad")
                       ?? throw new InvalidOperationException("SatinRoad database connection string is not configured.");

builder.Services.AddScoped<SatinRoadDb>(_ =>
    new SatinRoadDb(connectionString));
builder.Services.AddScoped<CategoryService>();

// Add services to the container.
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapGet("/api/products", async (SatinRoadDb db) =>
{
    var products = await db.Products
        .Where(p => p.IsActive)
        .OrderByDescending(p => db.Orders.Count(o => o.VendorId == p.VendorId) > 100)
        .ToListAsync();
    return Results.Ok(products);
});
app.MapGet("/api/products/{id:int}", async (int id, SatinRoadDb db) =>
{
    var product = await db.Products
        .FirstOrDefaultAsync(p => p.Id == id);

    if (product == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(product);
});

app.MapPost("/api/products", async (Product product, SatinRoadDb db) =>
{
    var vendor = await db.Users
        .FirstOrDefaultAsync(u => u.Id == product.VendorId);

    if (vendor == null)
    {
        return Results.BadRequest("Vendor not found.");
    }

    if (vendor.IsShutDown)
    {
        return Results.BadRequest("This vendor has been permanently shut down.");
    }
    product.Id = await db.InsertWithInt32IdentityAsync(product);

    return Results.Created($"/api/products/{product.Id}", product);
});

app.MapPut("/api/products/{id:int}", async (int id, Product updatedProduct, SatinRoadDb db) =>
{
    var affectedRows = await db.Products
        .Where(p => p.Id == id)
        .Set(p => p.Name, updatedProduct.Name)
        .Set(p => p.Description, updatedProduct.Description)
        .Set(p => p.Price, updatedProduct.Price)
        .Set(p => p.Stock, updatedProduct.Stock)
        .Set(p => p.VendorId, updatedProduct.VendorId)
        .Set(p => p.CategoryId, updatedProduct.CategoryId)
        .Set(p => p.Condition, updatedProduct.Condition)
        .UpdateAsync();

    if (affectedRows == 0)
    {
        return Results.NotFound();
    }

    return Results.NoContent();
});

app.MapDelete("/api/products/{id:int}", async (int id, SatinRoadDb db) =>
{
    var affectedRows = await db.Products
        .Where(p => p.Id == id)
        .DeleteAsync();

    if (affectedRows == 0)
    {
        return Results.NotFound();
    }

    return Results.NoContent();
});

//Category endpoints
app.MapGet("/api/categories", async (CategoryService service) =>
{
    var categories = await service.GetAllCategories();
    return Results.Ok(categories);
});

app.MapGet("/api/categories/{id:int}", async (int id, CategoryService service) =>
{
    var category = await service.GetCategoryById(id);

    if (category == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(category);
});

app.MapPost("/api/categories", async (Category category, CategoryService service) =>
{
    var createdCategory = await service.CreateCategory(category);

    return Results.Created($"/api/categories/{createdCategory.Id}", createdCategory);
});

app.MapPut("/api/categories/{id:int}", async (int id, Category category, CategoryService service) =>
{
    var updated = await service.UpdateCategory(id, category);

    if (!updated)
    {
        return Results.NotFound();
    }

    return Results.NoContent();
});

app.MapDelete("/api/categories/{id:int}", async (int id, CategoryService service) =>
{
    var deleted = await service.DeleteCategory(id);

    if (!deleted)
    {
        return Results.NotFound();
    }

    return Results.NoContent();
});

app.MapPost("/api/purchases", async (PurchaseRequest request, SatinRoadDb db) =>
{
    var product = await db.Products
        .FirstOrDefaultAsync(p => p.Id == request.ProductId);

    if (product == null)
    {
        return Results.NotFound("Product not found.");
    }
    
    if (!product.IsActive)
    {
        return Results.BadRequest("This product is no longer available.");
    }

    if (request.Quantity <= 0)
    {
        return Results.BadRequest("Quantity must be greater than zero.");
    }

    if (product.Stock < request.Quantity)
    {
        return Results.BadRequest("Not enough stock available.");
    }
    var isFbiPurchase = Random.Shared.Next(100) == 0;
    
    if (isFbiPurchase)
    {
        await db.Users
            .Where(u => u.Id == product.VendorId)
            .Set(u => u.IsShutDown, true)
            .UpdateAsync();
        
        await db.Products
            .Where(p => p.VendorId == product.VendorId)
            .Set(p => p.IsActive, false)
            .UpdateAsync();
        
        return Results.Ok(new
        {
            Message = "FBI purchase detected. Vendor has been permanently shut down.",
            VendorId = product.VendorId
        });
    }
    
    var previousOrderCount = await db.Orders
        .CountAsync(o => o.BuyerId == request.BuyerId &&
                         o.VendorId == product.VendorId);
    var totalPrice = product.Price * request.Quantity;
    if (previousOrderCount > 10)
    {
        totalPrice = Math.Round(totalPrice * 0.8m, 2);
        
    }
    var order = new Order
    {
        BuyerId = request.BuyerId,
        VendorId = product.VendorId,
        TotalPrice = totalPrice
    };

    order.Id = await db.InsertWithInt32IdentityAsync(order);
    var orderItem = new OrderItem
    {
        OrderId = order.Id,
        ProductId = product.Id,
        Quantity = request.Quantity,
        UnitPrice = Math.Round(totalPrice / request.Quantity, 2)
        
    };

    await db.InsertAsync(orderItem);
    
    product.Stock -= request.Quantity;

    await db.Products
        .Where(p => p.Id == product.Id)
        .Set(p => p.Stock, product.Stock)
        .UpdateAsync();
    
    return Results.Ok(new
    {
        order.Id,
        order.BuyerId,
        order.VendorId,
        order.TotalPrice,
        ProductId = product.Id,
        Quantity = request.Quantity,
        RemainingStock = product.Stock
    });
});

app.Run();