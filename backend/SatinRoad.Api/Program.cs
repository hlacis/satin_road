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
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<PurchaseService>();

// Add services to the container.
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

    var app = builder.Build();
    app.UseCors("Frontend");

// Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();


// Product endpoints
    app.MapGet("/api/products", async (ProductService service) =>
        {
            var products = await service.GetAllProducts();

            return Results.Ok(products);
        })
        .Produces<IEnumerable<Product>>(StatusCodes.Status200OK);

    app.MapGet("/api/products/{id:int}", async (int id, ProductService service) =>
    {
        var product = await service.GetProductById(id);

        if (product == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(product);
    });

    app.MapPost("/api/products", async (Product product, ProductService service) =>
    {
        var createdProduct = await service.CreateProduct(product);

        if (createdProduct == null)
        {
            return Results.BadRequest("Vendor not found or has been permanently shut down.");
        }

        return Results.Created($"/api/products/{createdProduct.Id}", createdProduct);
    });

    app.MapPut("/api/products/{id:int}", async (int id, Product product, ProductService service) =>
    {
        var updated = await service.UpdateProduct(id, product);

        if (!updated)
        {
            return Results.NotFound();
        }

        return Results.NoContent();
    });

    app.MapDelete("/api/products/{id:int}", async (int id, ProductService service) =>
    {
        var deleted = await service.DeleteProduct(id);

        if (!deleted)
        {
            return Results.NotFound();
        }

        return Results.NoContent();
    });

// Category endpoints
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

// Purchase endpoint
    app.MapPost("/api/purchases", async (PurchaseRequest request, PurchaseService service) =>
    {
        var product = await service.GetAvailableProduct(
            request.ProductId,
            request.Quantity);

        if (product == null)
        {
            return Results.BadRequest("Product is not available or there is not enough stock.");
        }

        var isFbiPurchase = await service.CheckFbiPurchase(product);

        if (isFbiPurchase)
        {
            return Results.Ok(new
            {
                Message = "FBI purchase detected. Vendor has been permanently shut down.",
                VendorId = product.VendorId
            });
        }

        var totalPrice = await service.CalculateTotalPrice(request, product);

        var order = await service.CreateOrder(
            request,
            product,
            totalPrice);

        await service.UpdateStock(product, request.Quantity);

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
    })
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest);

    app.Run();
