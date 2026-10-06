using LinqToDB;
using LinqToDB.Data;
using SatinRoad.Api.Models;


namespace SatinRoad.Api.Data;

public class SatinRoadDb : DataConnection
{
    public SatinRoadDb(string connectionString)
        : base(new DataOptions().UseSqlServer(connectionString))
    {
    }
    public ITable<Product> Products => this.GetTable<Product>();
    public ITable<Category> Categories => this.GetTable<Category>();
    public ITable<Order> Orders => this.GetTable<Order>();
    public ITable<OrderItem> OrderItems => this.GetTable<OrderItem>();
    public ITable<User> Users => this.GetTable<User>();
}