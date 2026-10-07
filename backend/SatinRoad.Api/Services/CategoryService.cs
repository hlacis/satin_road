using LinqToDB;
using LinqToDB.Async;
using SatinRoad.Api.Data;
using SatinRoad.Api.Models;
using SatinRoad.Api.Results;

namespace SatinRoad.Api.Services;


// Handles category related database operations
public class CategoryService
{
    private readonly SatinRoadDb _db;

    public CategoryService(SatinRoadDb db)
    {
        _db = db;
    }

    public async Task<List<Category>> GetAllCategories()
    {
        return await _db.Categories.ToListAsync();
    }
    public async Task<Category?> GetCategoryById(int id)
    {
        return await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    public async Task<(CategoryResult Result, Category? Category)> CreateCategory(Category category)
    {
        // Validate basic category data
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            return (CategoryResult.Invalid, null);
        }
        category.Id = await _db.InsertWithInt32IdentityAsync(category);

        return (CategoryResult.Success, category);
    }
    public async Task<CategoryResult> UpdateCategory(int id, Category category)
    {
        // Validate basic category data
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            return CategoryResult.Invalid;
        }

        var existingCategory = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == id);
        if (existingCategory == null)
        {
            return CategoryResult.NotFound;
        }

        var affectedRows = await _db.Categories
            .Where(c => c.Id == id)
            .Set(c => c.Name, category.Name)
            .UpdateAsync();

        return affectedRows > 0
            ? CategoryResult.Success
            : CategoryResult.NotFound;
    }
    public async Task<bool> DeleteCategory(int id)
    {
        var affectedRows = await _db.Categories
            .Where(c => c.Id == id)
            .DeleteAsync();

        return affectedRows > 0;
    }
}