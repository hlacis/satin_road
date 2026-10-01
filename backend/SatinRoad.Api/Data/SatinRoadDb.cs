using LinqToDB;
using LinqToDB.Data;

namespace SatinRoad.Api.Data;

public class SatinRoadDb : DataConnection
{
    public SatinRoadDb(string connectionString)
        : base(new DataOptions().UseSqlServer(connectionString))
    {
    }
}