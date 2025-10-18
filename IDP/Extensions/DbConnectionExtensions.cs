using Dapper;
using System.Data;
namespace IDP.Extensions
{
    public static class DbConnectionExtensions
    {
        public static IEnumerable<T> QueryFromFile<T>(this IDbConnection connection, string queryPath, object parameters, int timeout=120)
        {
            string query = ReadQuery(queryPath);

            return connection.Query<T>(query, parameters, commandTimeout: timeout);
        }

        public static T QuerySingleFromFile<T>(this IDbConnection connection, string queryPath, object parameters, int timeout = 120)
        {
            string query = ReadQuery(queryPath);

            return connection.QueryFirstOrDefault<T>(query, parameters, commandTimeout: timeout) ?? throw new Exception(nameof(T) + " not found");
        }

        private static int ExecuteFromFile(this IDbConnection connection, string queryPath, object parameters, int timeout = 120)
        {
            string query = ReadQuery(queryPath);

            return connection.Execute(query, parameters, commandTimeout: timeout);
        }

        private static string ReadQuery(string queryPath)
        {
            return File.ReadAllText(queryPath);
        }
    }
}
