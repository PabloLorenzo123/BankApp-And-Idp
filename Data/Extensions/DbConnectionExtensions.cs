using Dapper;
using System.Data;
namespace Data.Extensions
{
    public static class DbConnectionExtensions
    {
        public static IEnumerable<T> QueryFromFile<T>(this IDbConnection connection, string queryPath, object? parameters, int timeout=120)
        {
            string query = ReadQuery(queryPath);

            return connection.Query<T>(query, parameters, commandTimeout: timeout);
        }

        public static T QuerySingleFromFile<T>(this IDbConnection connection, string queryPath, object parameters, int timeout = 120)
        {
            string query = ReadQuery(queryPath);

            return connection.QueryFirstOrDefault<T>(query, parameters, commandTimeout: timeout) ?? throw new Exception(nameof(T) + " not found");
        }

        public static int ExecuteFromFile(this IDbConnection connection, string queryPath, object parameters, int timeout = 120)
        {
            string query = ReadQuery(queryPath);

            try
            {
                return connection.Execute(query, parameters, commandTimeout: timeout);
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        private static string ReadQuery(string queryPath)
        {
            return File.ReadAllText(queryPath);
        }
    }
}
