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
}