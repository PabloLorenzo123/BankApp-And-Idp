using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Data
{
    public class ConnectionFactory(IConfiguration configuration)
    {
        public IDbConnection CreateConnection(Connections connection)
            => connection switch
            {
                Connections.IDP => new SqliteConnection(configuration.GetConnectionString("IDP") 
                    ?? throw new Exception("Connection string for the IDP database is not provided in settings.json")),
                Connections.Bank => new SqliteConnection(configuration.GetConnectionString("Bank")
                    ?? throw new Exception("Connection string for the IDP database is not provided in settings.json")),
                _ => throw new NotImplementedException()
            };
    }
}
