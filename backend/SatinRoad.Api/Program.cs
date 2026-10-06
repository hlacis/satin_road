using LinqToDB;
using LinqToDB.Async;
using SatinRoad.Api.Data;
using SatinRoad.Api.Models;



var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SatinRoad")
                       ?? throw new InvalidOperationException("SatinRoad database connection string is not configured.");

builder.Services.AddScoped<SatinRoadDb>(_ =>
    new SatinRoadDb(connectionString));

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
    var products = await db.Products.ToListAsync();

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

app.Run();