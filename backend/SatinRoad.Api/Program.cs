using LinqToDB;
using LinqToDB.Async;
using SatinRoad.Api.Data;
using SatinRoad.Api.Models;
using SatinRoad.Api.Services;
using SatinRoad.Api.Results;
using Microsoft.Extensions.FileProviders;



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


// Handles unexpected errors and returns a safe error response
builder.Services.AddProblemDetails();

var app = builder.Build();

var webRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
Directory.CreateDirectory(webRoot);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(webRoot)
});

app.UseCors("Frontend");


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();

// Handles unexpected exceptions globally
app.UseExceptionHandler();

// Product endpoints
app.MapGet("/api/products", async (ProductService service) =>
    {
        var products = await service.GetAllProducts();

        return Results.Ok(products);
    })
    .Produces<List<Product>>(StatusCodes.Status200OK);

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
    var result = await service.CreateProduct(product);

    if (result.Result == ProductResult.Invalid)
    {
        return Results.BadRequest("Invalid product data.");
    }
    if (result.Result == ProductResult.VendorNotFound)
    {
        return Results.NotFound("Vendor not found or has been permanently shut down.");
    }

    if (result.Result == ProductResult.CategoryNotFound)
    {
        return Results.NotFound("Category not found.");
    }
    return Results.Created(
        $"/api/products/{result.Product!.Id}",
        result.Product);
});

app.MapPut("/api/products/{id:int}", async (int id, Product product, ProductService service) =>
{
    var result = await service.UpdateProduct(id, product);
    if (result == ProductResult.Invalid)
    {
        return Results.BadRequest("Invalid product data.");
    }
    if (result == ProductResult.VendorNotFound)
    {
        return Results.NotFound("Vendor not found or has been permanently shut down.");
    }

    if (result == ProductResult.CategoryNotFound)
    {
        return Results.NotFound("Category not found.");
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
    })
    .Produces<List<Category>>(StatusCodes.Status200OK);

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
    var result = await service.CreateCategory(category);

    if (result.Result == CategoryResult.Invalid)
    {
        return Results.BadRequest("Invalid category data.");
    }

    return Results.Created(
        $"/api/categories/{result.Category!.Id}",
        result.Category);
});

app.MapPut("/api/categories/{id:int}", async (int id, Category category, CategoryService service) =>
{
    var result = await service.UpdateCategory(id, category);

    if (result == CategoryResult.Invalid)
    {
        return Results.BadRequest("Invalid category data.");
    }
    if (result == CategoryResult.NotFound)
    {
        return Results.NotFound("Category not found.");
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
    var isValidBuyer = await service.IsValidBuyer(request.BuyerId);

    if (!isValidBuyer)
    {
        return Results.BadRequest("Buyer not found or has been permanently shut down.");
    }
    var result = await service.GetAvailableProduct(
        request.ProductId,
        request.Quantity);

    if (result.Result == PurchaseResult.ProductNotFound)
    {
        return Results.NotFound("Product not found.");
    }
    if (result.Result == PurchaseResult.InvalidQuantity)
    {
        return Results.BadRequest("Quantity must be greater than zero.");
    }
    if (result.Result == PurchaseResult.InsufficientStock)
    {
        return Results.BadRequest("Not enough stock available.");
    }

    var product = result.Product!;

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
});

app.MapPost("/api/uploads", async (IFormFile file, IWebHostEnvironment environment) =>
    {
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };

        if (!allowedTypes.Contains(file.ContentType))
            return Results.BadRequest("Only JPG, PNG and WebP images are allowed.");

        if (file.Length == 0 || file.Length > 5 * 1024 * 1024)
            return Results.BadRequest("Image must be between 1 byte and 5 MB.");

        var extension = file.ContentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ""
        };

        var uploadsDirectory = Path.Combine(environment.ContentRootPath, "wwwroot", "uploads");

        Directory.CreateDirectory(uploadsDirectory);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsDirectory, fileName);

        await using (var stream = File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        return Results.Ok(new
        {
            imageUrl = $"/uploads/{fileName}"
        });
    })
    .DisableAntiforgery();

app.Run();