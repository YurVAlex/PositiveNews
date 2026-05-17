using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace PositiveNews.Infrastructure.Persistence.Connection;

/// <summary>
/// Resolves a SQL Server connection string by probing connectivity against configured candidates.
/// </summary>
public static class ConnectionStringResolver
{
    /// <summary>
    /// Returns <c>DefaultConnection</c> if reachable; otherwise <c>AlternativeConnection</c> if reachable.
    /// </summary>
    /// <param name="configuration">Application configuration containing connection strings.</param>
    /// <returns>A connection string that successfully opens to the server (<c>master</c> database).</returns>
    /// <exception cref="InvalidOperationException">Thrown when neither candidate can be opened.</exception>
    public static string Resolve(IConfiguration configuration)
    {
        var defaultConn = configuration.GetConnectionString("DefaultConnection");
        var altConn = configuration.GetConnectionString("AlternativeConnection");

        if (CanConnectToServer(defaultConn))
            return defaultConn!;

        if (CanConnectToServer(altConn))
            return altConn!;

        throw new InvalidOperationException(
            "Unable to connect to either DefaultConnection or AlternativeConnection.");
    }

    private static bool CanConnectToServer(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return false;

        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master"
            };

            using var connection = new SqlConnection(builder.ConnectionString);
            connection.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
