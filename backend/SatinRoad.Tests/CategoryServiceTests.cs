using NUnit.Framework;
using SatinRoad.Api.Models;
using SatinRoad.Api.Results;
using SatinRoad.Api.Services;

namespace SatinRoad.Tests;

public class CategoryServiceTests
{
    // A category without a name should be rejected
    [Test]
    public async Task CreateCategory_WithEmptyName_ReturnsInvalid()
    {
        var service = new CategoryService(null!);

        var category = new Category
        {
            Name = ""
        };

        var result = await service.CreateCategory(category);

        Assert.That(result.Result, Is.EqualTo(CategoryResult.Invalid));
        Assert.That(result.Category, Is.Null);
    }
}