using LinqToDB;
using LinqToDB.Async;
using SatinRoad.Api.Data;
using SatinRoad.Api.Models;

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
    public async Task<Category> CreateCategory(Category category)
    {
        category.Id = await _db.InsertWithInt32IdentityAsync(category);

        return category;
    }
    public async Task<bool> UpdateCategory(int id, Category category)
    {
        var affectedRows = await _db.Categories
            .Where(c => c.Id == id)
            .Set(c => c.Name, category.Name)
            .UpdateAsync();

        return affectedRows > 0;
    }
    public async Task<bool> DeleteCategory(int id)
    {
        var affectedRows = await _db.Categories
            .Where(c => c.Id == id)
            .DeleteAsync();

        return affectedRows > 0;
    }
}