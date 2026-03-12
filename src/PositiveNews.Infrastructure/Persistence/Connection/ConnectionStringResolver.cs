using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace PositiveNews.Infrastructure.Persistence.Connection;

public static class ConnectionStringResolver
{
    public static string Resolve(IConfiguration configuration)
    {
        var defaultConn = configuration.GetConnectionString("DefaultConnection");
        var altConn = configuration.GetConnectionString("AlternativeConnection");

        if (CanConnect(defaultConn))
            return defaultConn!;

        return altConn!;
    }

    private static bool CanConnect(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return false;

        try
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }
}